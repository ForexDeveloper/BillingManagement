using System;
using System.Linq;
using System.Threading;
using Domain.Core.Enums;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Domain.Core.Entities.B2bInstallmentAggregate;
using Domain.Core.Entities.MerchantBillingAggregate;
using Domain.Core.Entities.FinancialDocumentAggregate;
using Domain.Core.Entities.MerchantInstallmentAggregate;
using Domain.Core.Entities.MerchantBillingAggregate.ValueObjects;
using Domain.Core.Entities.MerchantInstallmentAggregate.ValueObjects;
using static MassTransit.ValidationResultExtensions;
using Domain.Core.Entities.AccountAggregate;
using Domain.Core.Entities.InstallmentAggregate;
using System.Diagnostics.Contracts;
using Domain.Core.Entities.TenantMerchantContractAggregate;

namespace Infrastructure.Data.Repository.EfCore.Repositories;

public sealed class MerchantInstallmentRepository(ApplicationDbContext applicationDbContext) : IMerchantInstallmentRepository
{
    public async Task AddRangeAsync(List<MerchantInstallment> installments)
    {
        await applicationDbContext.MerchantInstallments.AddRangeAsync(installments);
    }

    public async Task<List<GroupIdentityInstallments>> GetGroupIdentityContractInstallments(CancellationToken cancellationToken)
    {
        //Should be converted to Persian ???
        var today = DateTime.Today;

        return await applicationDbContext.MerchantInstallments
            .Where(p => p.Type == B2bInstallmentType.Installment)
            .Where(p => p.BillingId.HasValue == false && p.DueDate <= today)
            .GroupBy(p => new
            {
                p.FromBusinessIdentityId,
                p.ToBusinessIdentityId,
                p.TenantMerchantContract.BillingPeriod,
                p.TenantMerchantContract.BillingPeriodType,
                p.TenantMerchantContract.DailyBillingOriginDate
            })
            .Select(p => new GroupIdentityInstallments()
            {
                BillingPeriod = p.Key.BillingPeriod,
                BillingPeriodType = p.Key.BillingPeriodType,
                ToBusinessIdentityId = p.Key.ToBusinessIdentityId,
                DailyBillingOriginDate = p.Key.DailyBillingOriginDate,
                FromBusinessIdentityId = p.Key.FromBusinessIdentityId,
                Installments = p.Select(installment => installment)
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<GroupIdentityInstallments>> GetGroupIdentityInstallments(CancellationToken cancellationToken)
    {
        //Should be converted to Persian ???
        var today = DateTime.Today;

        return await applicationDbContext.MerchantInstallments
            .Include(p => p.TenantMerchantContract)
            .Where(p => p.Type == B2bInstallmentType.Installment)
            .Where(p => p.BillingId.HasValue == false && p.DueDate <= today)
            .GroupBy(p => new { p.FromBusinessIdentityId, p.ToBusinessIdentityId })
            .Select(p => new GroupIdentityInstallments()
            {
                FromBusinessIdentityId = p.Key.FromBusinessIdentityId,
                ToBusinessIdentityId = p.Key.ToBusinessIdentityId,
                Installments = p.Select(installment => installment)
            })
            .ToListAsync(cancellationToken);
    }

    public async Task Calculate(CancellationToken cancellationToken)
    {
        var pc = new PersianCalendar();

        var today = DateTime.Today;

        var year = pc.GetYear(today);
        var month = pc.GetMonth(today);
        var day = pc.GetDayOfMonth(today);
        var dayOfWeek = pc.GetDayOfWeek(today);
        var daysInMonth = pc.GetDaysInMonth(year, month);

        var billingRanges = new Dictionary<BillingRangeKey, BillingRangeValue>();

        var billingRangesTuple = new Dictionary<(int Period, TimeInterval PeriodType, DateTime? OriginDate), (DateTime StartDate, DateTime EndDate)>();

        var dailyIds = new List<int>();

        var dailyDates = new List<DateTime>();

        var billings = new List<MerchantBilling>();

        var billingDtos = new List<BillingDto>();

        // 29 , 31  Exceptions

        var dailyPeriods = await applicationDbContext.TenantMerchantContracts
            .Where(p => p.BillingPeriodType == TimeInterval.Day)
            .Select(p => new
            {
                p.Id,
                p.BillingPeriod,
                p.DailyBillingOriginDate
            })
            .DistinctBy(p => new { p.BillingPeriod, p.DailyBillingOriginDate })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        foreach (var dailyPeriod in dailyPeriods)
        {
            if (!dailyPeriod.DailyBillingOriginDate.HasValue) continue;

            var originDate = dailyPeriod.DailyBillingOriginDate.Value;

            if (today.Date < originDate.Date) continue;

            var difference = (today.Date - originDate.Date).Days;

            if (difference % dailyPeriod.BillingPeriod == 0)
            {
                dailyIds.Add(dailyPeriod.Id);
            }
        }

        var allContracts = await applicationDbContext.TenantMerchantContracts.GroupBy(p => new
        {
            p.BillingPeriod,
            p.BillingPeriodType,
            p.DailyBillingOriginDate
        })
        .Select(p => new
        {
            p.Key.BillingPeriod,
            p.Key.BillingPeriodType,
            p.Key.DailyBillingOriginDate,
            ContractIds = p.Select(q => q.Id),
            TenantMerchantIds = p.GroupBy(q => new { q.TenantId, q.MerchantId }).Select(q => new { q.Key.TenantId, q.Key.MerchantId })
        })
        .AsNoTracking()
        .ToListAsync(cancellationToken);

        var i = 0;

        var currentPeriod = false;

        bool shouldCheckPreviousPeriod = true;

        var finalQuery = new FinalInstallmentQuery();

        foreach (var contract in allContracts)
        {
            var difference = 0;

            DateTime endOfContract;
            DateTime startOfContract;

            var period = contract.BillingPeriod;

            switch (contract.BillingPeriodType)
            {
                case TimeInterval.Day:

                    if (!contract.DailyBillingOriginDate.HasValue) continue;

                    var originDate = contract.DailyBillingOriginDate.Value;

                    if (today.Date < originDate.Date) continue;

                    var totalDays = (today.Date - originDate.Date).Days;

                    difference = totalDays % period;

                    endOfContract = pc.AddDays(new DateTime(year, month, day, pc), -difference);

                    startOfContract = pc.AddDays(endOfContract, -period);

                    break;

                case TimeInterval.Week:

                    if (dayOfWeek >= (DayOfWeek)period)
                    {
                        difference = (int)dayOfWeek - period;
                    }
                    else
                    {
                        difference = ((int)dayOfWeek - period) + 7;
                    }

                    endOfContract = pc.AddDays(new DateTime(year, month, day, pc), -difference);

                    startOfContract = pc.AddWeeks(endOfContract, -1);

                    break;

                case TimeInterval.Month:

                    if (day >= period)
                    {
                        endOfContract = pc.ToDateTime(year, month, period, 0, 0, 0, 0);
                    }
                    else
                    {
                        endOfContract = pc.AddMonths(new DateTime(year, month, period, pc), -1);
                    }

                    startOfContract = pc.AddMonths(endOfContract, -1);

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            var installmentQuery = applicationDbContext.MerchantInstallments
                   .Where(p => p.BillingId.HasValue == false)
                   .Where(p => startOfContract <= p.DueDate && p.DueDate < endOfContract)
                   .Where(p => contract.ContractIds.Contains(p.TenantMerchantContractId));

            var financialDocumentQuery = applicationDbContext.FinancialDocuments
                   .Where(p => startOfContract <= p.CreatedDateTime && p.CreatedDateTime < endOfContract)
                   .Where(p => contract.ContractIds.Contains(p.TenantMerchantContractId.Value));

            if (difference == 0)
            {
                foreach (var item in contract.TenantMerchantIds)
                {
                    var billing = new MerchantBilling(item.TenantId, item.TenantId, item.MerchantId,
                        BillingType.TenantToMerchant, 0, 0, 0, 0, startOfContract,
                        endOfContract, 0, 0, 0, 0, 0,
                        null, null);

                    billings.Add(billing);

                    currentPeriod = true;
                }

                if (i == 0)
                {
                    finalQuery.InstallmentQuery = installmentQuery;

                    finalQuery.FinancialDocumentQuery = financialDocumentQuery;
                }
                else
                {
                    finalQuery.InstallmentQuery = finalQuery.InstallmentQuery.Union(installmentQuery);

                    finalQuery.FinancialDocumentQuery = finalQuery.FinancialDocumentQuery.Union(financialDocumentQuery);
                }
            }
            else
            {
                var billingQuery = applicationDbContext.MerchantBillings
                    .Where(p => startOfContract == p.StartDate && endOfContract == p.EndDate);

                var result = await billingQuery.AnyAsync(cancellationToken);

                if (!result)
                {
                    foreach (var item in contract.TenantMerchantIds)
                    {
                        var billing = new MerchantBilling(item.TenantId, item.TenantId, item.MerchantId,
                            BillingType.TenantToMerchant, 0, 0, 0, 0, startOfContract,
                            endOfContract, 0, 0, 0, 0, 0,
                            null, null);

                        var billingDto = new BillingDto
                        {
                            Billing = billing,
                            ContractGroup = new ContractGroup
                            {
                                TenantId = item.TenantId,
                                MerchantId = item.MerchantId,
                                ContractIds = contract.ContractIds,
                                BillingPeriod = contract.BillingPeriod,
                                BillingPeriodType = contract.BillingPeriodType,
                                DailyBillingOriginDate = contract.DailyBillingOriginDate
                            }
                        };

                        billings.Add(billing);
                        billingDtos.Add(billingDto);
                    }

                    if (i == 0)
                    {
                        finalQuery.InstallmentQuery = installmentQuery;

                        finalQuery.FinancialDocumentQuery = financialDocumentQuery;
                    }
                    else
                    {
                        finalQuery.InstallmentQuery = finalQuery.InstallmentQuery.Union(installmentQuery);

                        finalQuery.FinancialDocumentQuery = finalQuery.FinancialDocumentQuery.Union(financialDocumentQuery);
                    }

                    await CheckPreviousPeriod(finalQuery, contract.BillingPeriod, contract.BillingPeriodType,
                        contract.ContractIds, startOfContract, endOfContract,
                        pc, cancellationToken);
                }
            }

            i++;
            currentPeriod = false;
        }

        var contractPeriods = await applicationDbContext.TenantMerchantContracts.GroupBy(p => new
        {
            p.BillingPeriod,
            p.BillingPeriodType,
            p.DailyBillingOriginDate
        }).Where(p =>
            (p.Key.BillingPeriodType == TimeInterval.Day && p.Select(q => q.Id).Any(q => dailyIds.Contains(q))) ||
            (p.Key.BillingPeriodType == TimeInterval.Month && p.Key.BillingPeriod == day) ||
            (p.Key.BillingPeriodType == TimeInterval.Month && p.Key.BillingPeriod >= 30 && daysInMonth == 29 && day == 29) ||
            (p.Key.BillingPeriodType == TimeInterval.Month && p.Key.BillingPeriod == 31 && daysInMonth == 30 && day == 30) ||
            (p.Key.BillingPeriodType == TimeInterval.Week && p.Key.BillingPeriod == (int)dayOfWeek))
        .Select(p => new
        {
            p.Key.BillingPeriod,
            p.Key.BillingPeriodType,
            p.Key.DailyBillingOriginDate,
            ContractIds = p.Select(q => q.Id)
        })
         .AsNoTracking()
         .ToListAsync(cancellationToken);

        if (!contractPeriods.Any()) return;

        foreach (var contractPeriod in contractPeriods)
        {
            DateTime billingStartDate;
            DateTime? previousBillingStartDate = null;
            DateTime? previousBillingEndDate = null;

            var period = contractPeriod.BillingPeriod;
            var periodType = contractPeriod.BillingPeriodType;
            var originDate = contractPeriod.DailyBillingOriginDate;

            switch (periodType)
            {
                case TimeInterval.Day:
                    billingStartDate = pc.AddDays(new DateTime(year, month, day, pc), -period);

                    if (shouldCheckPreviousPeriod)
                    {
                        previousBillingEndDate = pc.AddDays(new DateTime(year, month, day, pc), -period * (i + 1));
                        previousBillingStartDate = pc.AddDays(new DateTime(year, month, day, pc), -period * (i + 2));
                    }

                    break;

                case TimeInterval.Week:
                    billingStartDate = pc.AddWeeks(new DateTime(year, month, day, pc), -1);

                    if (shouldCheckPreviousPeriod)
                    {
                        previousBillingEndDate = pc.AddWeeks(new DateTime(year, month, day, pc), -1 * (i + 1));
                        previousBillingStartDate = pc.AddWeeks(new DateTime(year, month, day, pc), -1 * (i + 2));
                    }

                    break;

                case TimeInterval.Month:
                    billingStartDate = pc.AddMonths(new DateTime(year, month, day, pc), -1);

                    if (shouldCheckPreviousPeriod)
                    {
                        previousBillingEndDate = pc.AddMonths(new DateTime(year, month, day, pc), -1 * (i + 1));
                        previousBillingStartDate = pc.AddMonths(new DateTime(year, month, day, pc), -1 * (i + 2));
                    }

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            var billingEndDate = pc.ToDateTime(year, month, day, 0, 0, 0, 0);

            var installmentQuery = applicationDbContext.MerchantInstallments
                  .Where(p => p.BillingId.HasValue == false)
                  .Where(p => billingStartDate <= p.DueDate && p.DueDate < billingEndDate)
                  .Where(p => contractPeriod.ContractIds.Contains(p.TenantMerchantContractId));

            var financialDocumentQuery = applicationDbContext.FinancialDocuments
                   .Where(p => billingStartDate <= p.CreatedDateTime && p.CreatedDateTime < billingEndDate)
                   .Where(p => contractPeriod.ContractIds.Contains(p.TenantMerchantContractId.Value));

            var result = await installmentQuery.AnyAsync(cancellationToken);

            if (result)
            {
                if (i == 0)
                {
                    finalQuery.InstallmentQuery = installmentQuery;

                    finalQuery.FinancialDocumentQuery = financialDocumentQuery;
                }
                else
                {
                    finalQuery.InstallmentQuery = finalQuery.InstallmentQuery.Union(installmentQuery);

                    finalQuery.FinancialDocumentQuery = finalQuery.FinancialDocumentQuery.Union(financialDocumentQuery);
                }

                await CheckPreviousPeriod(finalQuery, contractPeriod.BillingPeriod, contractPeriod.BillingPeriodType, contractPeriod.ContractIds, billingStartDate, billingEndDate,
                                           pc, cancellationToken);
            }

            i++;
        }

        var finalInstallmentQuery = finalQuery.InstallmentQuery;
        var finalFinancialDocumentQuery = finalQuery.FinancialDocumentQuery;

        if (finalInstallmentQuery == null) return;

        var installmentsGroup = await finalInstallmentQuery.GroupBy(p => new
        {
            p.TenantMerchantContract.BillingPeriod,
            p.TenantMerchantContract.BillingPeriodType,
            p.TenantMerchantContract.DailyBillingOriginDate
        }).ToDictionaryAsync(
            p => new BillingRangeKey(p.Key.BillingPeriod, p.Key.BillingPeriodType, p.Key.DailyBillingOriginDate),
            p => p.Select(installment => installment).ToList(), cancellationToken);

        var financialDocumentsGroup = await finalFinancialDocumentQuery.GroupBy(p => new
        {
            p.TenantMerchantContract.BillingPeriod,
            p.TenantMerchantContract.BillingPeriodType,
            p.TenantMerchantContract.DailyBillingOriginDate
        }).ToDictionaryAsync(
            p => new BillingRangeKey(p.Key.BillingPeriod, p.Key.BillingPeriodType, p.Key.DailyBillingOriginDate),
            p => p.Select(financialDocument => financialDocument).ToList(), cancellationToken);

        var contracts = await applicationDbContext.TenantMerchantContracts.GroupBy(p => new
        {
            p.TenantId,
            p.MerchantId,
            p.BillingPeriod,
            p.BillingPeriodType,
            p.DailyBillingOriginDate
        }).Where(p =>
            (p.Key.BillingPeriodType == TimeInterval.Day && p.Select(q => q.Id).Any(q => dailyIds.Contains(q))) ||
            (p.Key.BillingPeriodType == TimeInterval.Month && p.Key.BillingPeriod == day) ||
            (p.Key.BillingPeriodType == TimeInterval.Month && p.Key.BillingPeriod >= 30 && daysInMonth == 29 && day == 29) ||
            (p.Key.BillingPeriodType == TimeInterval.Month && p.Key.BillingPeriod == 31 && daysInMonth == 30 && day == 30) ||
            (p.Key.BillingPeriodType == TimeInterval.Week && p.Key.BillingPeriod == (int)dayOfWeek))
        .Select(p => new ContractGroup
        {
            ContractIds = p.Select(q => q.Id),
            TenantId = p.Key.TenantId,
            MerchantId = p.Key.MerchantId,
            BillingPeriod = p.Key.BillingPeriod,
            BillingPeriodType = p.Key.BillingPeriodType,
            DailyBillingOriginDate = p.Key.DailyBillingOriginDate
        })
        .ToListAsync(cancellationToken);

        var notSettledBillings = await applicationDbContext.MerchantBillings
            .Where(p => (p.Status == BillingStatus.Issued || p.Status == BillingStatus.PartiallyPaid) && p.DueDate < DateTime.Today && !p.ParentId.HasValue)
            .Select(p => new NotSettledMerchantBilling
            {
                Billing = p,
                SumOfPayments = p.Payments.Sum(q => q.Amount),
                ContractIds = p.Installments.Cast<MerchantInstallment>().Select(q => q.TenantMerchantContractId)
            })
            .ToListAsync(cancellationToken);

        foreach (var billing in billingDtos)
        {
            decimal previousDebitAmount = 0;

            var contract = billing.ContractGroup;

            MerchantBilling overdueBilling = null;

            var billingRangeKey = new BillingRangeKey(contract.BillingPeriod, contract.BillingPeriodType, contract.DailyBillingOriginDate);

            var installments = installmentsGroup.GetValueOrDefault(billingRangeKey);

            var financialDocuments = financialDocumentsGroup.GetValueOrDefault(billingRangeKey);

            var billingStartDate = billingRanges.GetValueOrDefault(billingRangeKey).StartDate;

            var billingEndDate = billingRanges.GetValueOrDefault(billingRangeKey).EndDate;

            var amount = installments.Sum(p => p.Amount);

            var previousPeriodRefundedPurchases = financialDocuments.Where(p => p.Type == FinancialDocumentType.Refund).Sum(p => p.Amount);

            var currentPeriodPurchaseTransactions = financialDocuments.Where(p => p.Type == FinancialDocumentType.Purchase).Sum(p => p.Amount);

            var notSettledBilling = notSettledBillings.FirstOrDefault(p => p.ContractIds.Any(q => contract.ContractIds.Contains(q)));
        }

        foreach (var contract in contracts)
        {
            decimal previousDebitAmount = 0;

            MerchantBilling overdueBilling = null;

            var billingRangeKey = new BillingRangeKey(contract.BillingPeriod, contract.BillingPeriodType, contract.DailyBillingOriginDate);

            var installments = installmentsGroup.GetValueOrDefault(billingRangeKey);

            var financialDocuments = financialDocumentsGroup.GetValueOrDefault(billingRangeKey);

            var billingStartDate = billingRanges.GetValueOrDefault(billingRangeKey).StartDate;

            var billingEndDate = billingRanges.GetValueOrDefault(billingRangeKey).EndDate;

            var amount = installments.Sum(p => p.Amount);

            var previousPeriodRefundedPurchases = financialDocuments.Where(p => p.Type == FinancialDocumentType.Refund).Sum(p => p.Amount);

            var currentPeriodPurchaseTransactions = financialDocuments.Where(p => p.Type == FinancialDocumentType.Purchase).Sum(p => p.Amount);

            var notSettledBilling = notSettledBillings.FirstOrDefault(p => p.ContractIds.Any(q => contract.ContractIds.Contains(q)));

            if (notSettledBilling != null)
            {
                notSettledBilling.Billing.Overdue();
                overdueBilling = notSettledBilling.Billing;
                previousDebitAmount = notSettledBilling.CalculateDebitAmount();
            }

            var billing = new MerchantBilling(contract.TenantId, contract.TenantId, contract.MerchantId,
                BillingType.TenantToMerchant, amount, previousDebitAmount, 0, 0, billingStartDate,
                billingEndDate, 0, 0, 0, previousPeriodRefundedPurchases, currentPeriodPurchaseTransactions,
                installments, overdueBilling);

            billings.Add(billing);
        }

        await applicationDbContext.MerchantBillings.AddRangeAsync(billings, cancellationToken);
    }

    public async Task CheckPreviousPeriod(FinalInstallmentQuery finalQuery, int billingPeriod, TimeInterval billingPeriodType, IEnumerable<int> contractIds,
                                    DateTime startOfContract, DateTime endOfContract,
                                    PersianCalendar pc, CancellationToken cancellationToken)
    {
        switch (billingPeriodType)
        {
            case TimeInterval.Day:

                endOfContract = pc.AddDays(endOfContract, -billingPeriod);

                startOfContract = pc.AddDays(startOfContract, -billingPeriod);

                break;

            case TimeInterval.Week:

                endOfContract = pc.AddWeeks(endOfContract, -1);

                startOfContract = pc.AddWeeks(startOfContract, -1);

                break;

            case TimeInterval.Month:

                endOfContract = pc.AddMonths(endOfContract, -1);

                startOfContract = pc.AddMonths(startOfContract, -1);

                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        var billingQuery = applicationDbContext.MerchantBillings
            .Where(p => startOfContract == p.StartDate && endOfContract == p.EndDate);

        var installmentQuery = applicationDbContext.MerchantInstallments
            .Where(p => p.BillingId.HasValue == false)
            .Where(p => startOfContract <= p.DueDate && p.DueDate < endOfContract)
            .Where(p => contractIds.Contains(p.TenantMerchantContractId));

        var financialDocumentQuery = applicationDbContext.FinancialDocuments
            .Where(p => startOfContract <= p.CreatedDateTime && p.CreatedDateTime < endOfContract)
            .Where(p => contractIds.Contains(p.TenantMerchantContractId.Value));

        var result = await billingQuery.AnyAsync(cancellationToken);

        if (!result)
        {
            finalQuery.InstallmentQuery = finalQuery.InstallmentQuery.Union(installmentQuery);

            finalQuery.FinancialDocumentQuery = finalQuery.FinancialDocumentQuery.Union(financialDocumentQuery);

            await CheckPreviousPeriod(finalQuery, billingPeriod, billingPeriodType, contractIds, startOfContract,
                                      endOfContract, pc, cancellationToken);
        }
    }
}

public sealed record ContractGroup
{
    public int TenantId { get; set; }

    public int MerchantId { get; set; }

    public IEnumerable<int> ContractIds { get; set; }

    public int BillingPeriod { get; set; }

    public TimeInterval BillingPeriodType { get; set; }

    public DateTime? DailyBillingOriginDate { get; set; }

    //public IEnumerable<B2bInstallment> Installments { get; set; }

    //public IEnumerable<FinancialDocument> FinancialDocuments { get; set; }
}

public sealed record BillingRangeValue(DateTime StartDate, DateTime EndDate, IEnumerable<int> ContractIds)
{
    public DateTime StartDate { get; set; } = StartDate;

    public DateTime EndDate { get; set; } = EndDate;

    public IEnumerable<int> ContractIds { get; set; } = ContractIds;
}

public sealed record BillingRangeKey(
    int BillingPeriod,
    TimeInterval BillingPeriodType,
    DateTime? DailyBillingOriginDate)
{
    public int BillingPeriod { get; set; } = BillingPeriod;

    public TimeInterval BillingPeriodType { get; set; } = BillingPeriodType;

    public DateTime? DailyBillingOriginDate { get; set; } = DailyBillingOriginDate;
}

public sealed record FinalInstallmentQuery
{
    public IQueryable<MerchantBilling> BillingQuery { get; set; }

    public IQueryable<MerchantInstallment> InstallmentQuery { get; set; }

    public IQueryable<FinancialDocument> FinancialDocumentQuery { get; set; }
}

public sealed record BillingDto
{
    public MerchantBilling Billing { get; set; }

    public ContractGroup ContractGroup { get; set; }
}