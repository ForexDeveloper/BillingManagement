using System;
using System.Linq;
using System.Threading;
using Domain.Core.Enums;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;
using Application.Service.Helper;
using Application.Service.Contracts;
using Domain.Core.UnitOfWorkContracts;
using Domain.Core.Entities.BillingAggregate.Dtos;
using Domain.Core.Entities.MerchantBillingAggregate;
using Domain.Core.Entities.FinancialDocumentAggregate;
using Domain.Core.Entities.MerchantInstallmentAggregate;
using Domain.Core.Entities.TenantMerchantContractAggregate;
using Domain.Core.Entities.TenantMerchantContractAggregate.Dtos;

namespace Application.Service.Services;

public sealed class MerchantBillingService(
    IApplicationDbContextUnitOfWork unitOfWork,
    IMerchantBillingRepository merchantBillingRepository,
    IFinancialDocumentRepository financialDocumentRepository,
    IMerchantInstallmentRepository merchantInstallmentRepository,
    ITenantMerchantContractRepository tenantMerchantContractRepository) : IMerchantBillingService
{
    public async Task IssueOrOverdueBilling(CancellationToken cancellationToken)
    {
        var today = DateTime.Today;

        var pc = new PersianCalendar();

        var year = pc.GetYear(today);
        var month = pc.GetMonth(today);
        var dayOfWeek = pc.GetDayOfWeek(today);
        var dayOfMonth = pc.GetDayOfMonth(today);
        var daysInMonth = pc.GetDaysInMonth(year, month);

        var finalQuery = new FinalInstallmentQuery();

        var billingGroups = new List<List<BillingDto>>();

        var allContracts = await tenantMerchantContractRepository.GetAllGroupContractAsync(cancellationToken);

        var overdueOrNotSettledBillings = await merchantBillingRepository.GetOverdueOrNotSettledBillings(cancellationToken);

        foreach (var contract in allContracts)
        {
            int difference;
            bool currentPeriod;
            DateTime endOfPeriod;
            DateTime startOfPeriod;

            var period = contract.BillingPeriod;

            var billingDtos = new List<BillingDto>();

            switch (contract.BillingPeriodType)
            {
                case TimeInterval.Day:

                    if (!contract.BillingDailyOriginDate.HasValue) continue;

                    var originDate = contract.BillingDailyOriginDate.Value;

                    if (today.Date < originDate.Date) continue;

                    var totalDays = (today.Date - originDate.Date).Days;

                    difference = period - (totalDays % period);

                    endOfPeriod = pc.AddDays(new DateTime(year, month, dayOfMonth, pc), difference);

                    startOfPeriod = pc.AddDays(endOfPeriod, -period);

                    currentPeriod = totalDays % period == 0;

                    break;

                case TimeInterval.Week:

                    if ((DayOfWeek)period >= dayOfWeek)
                    {
                        difference = period - (int)dayOfWeek;
                    }
                    else
                    {
                        difference = 7 - ((int)dayOfWeek - period);
                    }

                    endOfPeriod = pc.AddDays(new DateTime(year, month, dayOfMonth, pc), difference);

                    startOfPeriod = pc.AddWeeks(endOfPeriod, -1);

                    currentPeriod = (DayOfWeek)period == dayOfWeek;

                    break;

                case TimeInterval.Month:

                    period = DateHelper.RegulateBillingPeriod(daysInMonth, period);

                    if (period >= dayOfMonth)
                    {
                        difference = period - dayOfMonth;

                        endOfPeriod = pc.AddDays(new DateTime(year, month, dayOfMonth, pc), difference);
                    }
                    else
                    {
                        endOfPeriod = pc.AddMonths(new DateTime(year, month, period, pc), 1);
                    }

                    startOfPeriod = pc.AddMonths(endOfPeriod, -1);

                    currentPeriod = period == dayOfMonth;

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            var installmentQuery = merchantInstallmentRepository.CreateJobInstallmentQuery(startOfPeriod, endOfPeriod, contract.ContractIds);

            var financialDocumentQuery = financialDocumentRepository.CreateJobFinancialDocumentQuery(startOfPeriod, endOfPeriod, contract.ContractIds);

            if (contract.HasEndorsement)
            {
                var installmentRanges = await merchantInstallmentRepository.GetInstallmentRanges(contract.ContractIds, cancellationToken);

                if (installmentRanges == null) continue;

                if (currentPeriod)
                {
                    if (installmentRanges.HasIntersection(startOfPeriod, endOfPeriod))
                    {
                        var billingDto = new BillingDto
                        {
                            ContractGroup = contract,
                            EndOfPeriod = endOfPeriod,
                            StartOfPeriod = startOfPeriod,
                            ContractIds = contract.ContractIds
                        };

                        billingDtos.Add(billingDto);

                        finalQuery.BuildQueries(installmentQuery, financialDocumentQuery);
                    }
                }

                CheckPreviousInstallments(finalQuery, billingDtos, contract, startOfPeriod, endOfPeriod, installmentRanges);
            }
            else
            {
                if (currentPeriod)
                {
                    var billingDto = new BillingDto
                    {
                        ContractGroup = contract,
                        EndOfPeriod = endOfPeriod,
                        StartOfPeriod = startOfPeriod,
                        ContractIds = contract.ContractIds
                    };

                    billingDtos.Add(billingDto);

                    finalQuery.BuildQueries(installmentQuery, financialDocumentQuery);
                }

                await CheckPreviousBilling(finalQuery, billingDtos, contract, startOfPeriod, endOfPeriod, cancellationToken);
            }

            billingGroups.Add(billingDtos);
        }

        var overdueBillings = OverdueThenFilterBillings(overdueOrNotSettledBillings);

        var installmentGroup = await merchantInstallmentRepository.GetGroupContractInstallments(
            finalQuery.InstallmentQuery, cancellationToken);

        var financialDocumentGroup = await financialDocumentRepository.GetGroupContractFinancialDocuments(
           finalQuery.FinancialDocumentQuery, cancellationToken);

        var billings = new List<MerchantBilling>();

        foreach (var billingGroup in billingGroups)
        {
            List<MerchantBilling> alternativeBillings = [];

            var contractIds = billingGroup.First().ContractIds;

            var overdueBilling = overdueBillings.FirstOrDefault(p => contractIds.Any(q => p.Billing.ContractIds.Contains(q)))?.Billing ??
                                 overdueBillings.FirstOrDefault(p => contractIds.Contains(p.FinalEndorsementContractId))?.Billing;

            foreach (var billingDto in billingGroup.OrderBy(p => p.EndOfPeriod))
            {
                decimal previousDebitAmount = 0;
                decimal currentPeriodFinalCommission;
                decimal currentPeriodCalculatedCommission;

                var contract = billingDto.ContractGroup;

                var billingKey = new ContractIdentifier(contract.TenantId, contract.MerchantId, contract.BillingPeriod,
                    contract.BillingPeriodType, contract.BillingDailyOriginDate, contract.CommissionCalculationType);

                var installments = installmentGroup.GetValueOrDefault(billingKey);

                var financialDocuments = financialDocumentGroup.GetValueOrDefault(billingKey);

                var refundFinancialDocuments = financialDocuments.Where(p => billingDto.StartOfPeriod <= p.CreatedDateTime && p.CreatedDateTime < billingDto.EndOfPeriod).Where(p => p.Type == FinancialDocumentType.Refund);

                var purchaseFinancialDocuments = refundFinancialDocuments.Select(p => p.Parent);

                var refundedTransactionsCommission = purchaseFinancialDocuments.Sum(p => p.Commission) ?? 0;

                var currentPeriodPurchaseTransactions = installments.Where(p => billingDto.StartOfPeriod <= p.DueDate && p.DueDate < billingDto.EndOfPeriod).Sum(p => p.Amount);

                var previousPeriodRefundedTransactions = financialDocuments.Where(p => billingDto.StartOfPeriod <= p.CreatedDateTime && p.CreatedDateTime < billingDto.EndOfPeriod).Where(p => p.Type == FinancialDocumentType.Refund).Sum(p => p.Amount);

                decimal totalAmount = financialDocuments
                    .Where(p => p.Type == FinancialDocumentType.Purchase)
                    .Where(p => billingDto.StartOfPeriod <= p.CreatedDateTime && p.CreatedDateTime < billingDto.EndOfPeriod)
                    .Sum(p => p.Amount);

                if (contract.CommissionCalculationType == CommissionCalculationType.UniformTiered)
                {
                    var totalTransactionsAmount = await financialDocumentRepository.GetPeriodTotalTransactionsAmount(contract.TenantId, contract.MerchantId,
                        billingDto.StartOfPeriod, billingDto.EndOfPeriod, cancellationToken);

                    totalAmount = totalTransactionsAmount;

                    var tieredCommission = contract.TieredCommissions.FirstOrDefault(p =>
                        p.FromAmount < totalTransactionsAmount && totalTransactionsAmount <= p.ToAmount);

                    if (tieredCommission == null)
                    {
                        var minTieredCommission = contract.TieredCommissions.MinBy(p => p.ToAmount);

                        var maxTieredCommission = contract.TieredCommissions.MaxBy(p => p.ToAmount);

                        if (totalTransactionsAmount <= minTieredCommission.FromAmount)
                        {
                            tieredCommission = minTieredCommission;
                        }

                        else if (totalTransactionsAmount > maxTieredCommission.ToAmount)
                        {
                            tieredCommission = maxTieredCommission;
                        }
                    }

                    if (tieredCommission == null) break;

                    currentPeriodCalculatedCommission = totalTransactionsAmount * tieredCommission.Percentage;

                    if (currentPeriodCalculatedCommission > tieredCommission.MaxAmount)
                    {
                        currentPeriodCalculatedCommission = tieredCommission.MaxAmount.Value;
                    }

                    if (currentPeriodCalculatedCommission < tieredCommission.MinAmount)
                    {
                        currentPeriodCalculatedCommission = tieredCommission.MinAmount.Value;
                    }
                }
                else
                {
                    currentPeriodCalculatedCommission = installments
                        .Where(p => billingDto.StartOfPeriod <= p.DueDate && p.DueDate < billingDto.EndOfPeriod)
                        .Sum(p => p.Commission) ?? 0;
                }

                if (currentPeriodCalculatedCommission > contract.PeriodMaxCommissionAmount)
                {
                    currentPeriodFinalCommission = contract.PeriodMaxCommissionAmount.Value;
                }

                else if (currentPeriodCalculatedCommission < contract.PeriodMinCommissionAmount)
                {
                    currentPeriodFinalCommission = contract.PeriodMinCommissionAmount.Value;
                }
                else
                {
                    currentPeriodFinalCommission = currentPeriodCalculatedCommission;
                }

                if (contract.CommissionCalculationType == CommissionCalculationType.UniformTiered)
                {
                    foreach (var financialDocument in financialDocuments
                                 .Where(p => p.Type == FinancialDocumentType.Purchase)
                                 .Where(p => billingDto.StartOfPeriod <= p.CreatedDateTime && p.CreatedDateTime < billingDto.EndOfPeriod))
                    {
                        var commission = currentPeriodFinalCommission * (financialDocument.Amount / totalAmount);

                        financialDocument.SetCommission(commission);
                    }
                }

                var alternativeBilling = alternativeBillings.LastOrDefault();

                if (alternativeBilling != null)
                {
                    previousDebitAmount = alternativeBilling.GetDebitAmount();

                    if (previousDebitAmount != 0)
                    {
                        alternativeBilling.Overdue();
                        overdueBilling = alternativeBilling;
                    }
                }

                var billing = new MerchantBilling(contract.TenantId, contract.TenantId, contract.MerchantId,
                    BillingType.TenantToMerchant, contract.BillingPeriodType, previousDebitAmount, 0, 0,
                    billingDto.StartOfPeriod, billingDto.EndOfPeriod, 0, billingDto.ContractIds,
                    currentPeriodCalculatedCommission, currentPeriodFinalCommission, refundedTransactionsCommission,
                    previousPeriodRefundedTransactions, currentPeriodPurchaseTransactions, installments,
                    overdueBilling);

                if (billing.Amount == 0 && contract.HasEndorsement)
                {
                    continue;
                }

                billings.Add(billing);

                alternativeBillings.Add(billing);
            }
        }

        await merchantBillingRepository.AddRangeAsync(billings, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task AddOrUpdateBilling(CancellationToken cancellationToken)
    {
        var today = DateTime.Today;

        var pc = new PersianCalendar();

        var year = pc.GetYear(today);
        var month = pc.GetMonth(today);
        var day = pc.GetDayOfMonth(today);
        var dayOfWeek = pc.GetDayOfWeek(today);
        var daysInMonth = pc.GetDaysInMonth(year, month);

        var finalQuery = new FinalInstallmentQuery();

        var billingGroups = new List<List<BillingDto>>();

        var notAssignedBillings = await merchantBillingRepository.GetOverdueOrNotSettledBillings(cancellationToken);

        var currentContracts = await tenantMerchantContractRepository.GetCurrentGroupContractsAsync(cancellationToken);

        foreach (var contract in currentContracts)
        {
            DateTime billingStartDate;

            var period = contract.BillingPeriod;

            var billingDtos = new List<BillingDto>();

            switch (contract.BillingPeriodType)
            {
                case TimeInterval.Day:
                    billingStartDate = pc.AddDays(new DateTime(year, month, day, pc), -period);
                    break;

                case TimeInterval.Week:
                    billingStartDate = pc.AddWeeks(new DateTime(year, month, day, pc), -1);
                    break;

                case TimeInterval.Month:
                    billingStartDate = pc.AddMonths(new DateTime(year, month, day, pc), -1);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            var billingEndDate = pc.ToDateTime(year, month, day, 0, 0, 0, 0);

            var installmentQuery = merchantInstallmentRepository.CreateJobInstallmentQuery(billingStartDate, billingEndDate, contract.ContractIds);

            var financialDocumentQuery = financialDocumentRepository.CreateJobFinancialDocumentQuery(billingStartDate, billingEndDate, contract.ContractIds);

            if (contract.HasEndorsement)
            {
                var installmentRanges = await merchantInstallmentRepository.GetInstallmentRanges(contract.ContractIds, cancellationToken);

                if (installmentRanges == null) continue;

                if (installmentRanges.HasIntersection(billingStartDate, billingEndDate))
                {
                    var billingDto = new BillingDto
                    {
                        ContractGroup = contract,
                        EndOfPeriod = billingEndDate,
                        StartOfPeriod = billingStartDate,
                        ContractIds = contract.ContractIds
                    };

                    billingDtos.Add(billingDto);

                    finalQuery.BuildQueries(installmentQuery, financialDocumentQuery);
                }

                CheckPreviousInstallments(finalQuery, billingDtos, contract, billingStartDate, billingEndDate, installmentRanges);
            }
            else
            {
                var billingDto = new BillingDto
                {
                    ContractGroup = contract,
                    EndOfPeriod = billingEndDate,
                    StartOfPeriod = billingStartDate,
                    ContractIds = contract.ContractIds
                };

                billingDtos.Add(billingDto);

                finalQuery.BuildQueries(installmentQuery, financialDocumentQuery);

                await CheckPreviousBilling(finalQuery, billingDtos, contract, billingStartDate, billingEndDate, cancellationToken);
            }

            billingGroups.Add(billingDtos);
        }

        var overdueBillings = OverdueThenFilterBillings(notAssignedBillings);

        var installmentGroup = await merchantInstallmentRepository.GetGroupContractInstallments(
            finalQuery.InstallmentQuery, cancellationToken);

        var financialDocumentGroup = await financialDocumentRepository.GetGroupContractFinancialDocuments(
                finalQuery.FinancialDocumentQuery, cancellationToken);

        var billings = new List<MerchantBilling>();

        foreach (var billingGroup in billingGroups)
        {
            List<MerchantBilling> alternativeBillings = [];

            var contractIds = billingGroup.First().ContractIds;

            var overdueBilling = overdueBillings.FirstOrDefault(p => contractIds.Any(q => p.Billing.ContractIds.Contains(q)))?.Billing ??
                                 overdueBillings.FirstOrDefault(p => contractIds.Contains(p.FinalEndorsementContractId))?.Billing;

            foreach (var billingDto in billingGroup.OrderBy(p => p.EndOfPeriod))
            {
                decimal previousDebitAmount = 0;
                decimal currentPeriodFinalCommission;
                decimal currentPeriodCalculatedCommission;
                decimal refundedTransactionsCommission = 0;

                var contract = billingDto.ContractGroup;

                var endOfPeriod = billingDto.EndOfPeriod;

                var startOfPeriod = billingDto.StartOfPeriod;

                var billingKey = new ContractIdentifier(contract.TenantId, contract.MerchantId, contract.BillingPeriod,
                    contract.BillingPeriodType, contract.BillingDailyOriginDate, contract.CommissionCalculationType);

                var installments = installmentGroup.GetValueOrDefault(billingKey);

                var financialDocuments = financialDocumentGroup.GetValueOrDefault(billingKey);

                var currentPeriodPurchaseTransactions = installments.Where(p => startOfPeriod <= p.DueDate && p.DueDate < endOfPeriod).Sum(p => p.Amount);

                var previousPeriodRefundedTransactions = financialDocuments.Where(p => startOfPeriod <= p.CreatedDateTime && p.CreatedDateTime < endOfPeriod).Where(p => p.Type == FinancialDocumentType.Refund).Sum(p => p.Amount);

                if (contract.CommissionCalculationType == CommissionCalculationType.UniformTiered)
                {
                    var totalTransactionsAmount = await financialDocumentRepository.GetPeriodTotalTransactionsAmount(contract.TenantId, contract.MerchantId,
                        billingDto.StartOfPeriod, billingDto.EndOfPeriod, cancellationToken);

                    var tieredCommission = contract.TieredCommissions.FirstOrDefault(p =>
                        p.FromAmount < totalTransactionsAmount && totalTransactionsAmount <= p.ToAmount);

                    if (tieredCommission == null)
                    {
                        var minTieredCommission = contract.TieredCommissions.MinBy(p => p.ToAmount);

                        var maxTieredCommission = contract.TieredCommissions.MaxBy(p => p.ToAmount);

                        if (totalTransactionsAmount <= minTieredCommission.FromAmount)
                        {
                            tieredCommission = minTieredCommission;
                        }

                        else if (totalTransactionsAmount > maxTieredCommission.ToAmount)
                        {
                            tieredCommission = maxTieredCommission;
                        }
                    }

                    if (tieredCommission == null) break;

                    currentPeriodCalculatedCommission = totalTransactionsAmount * tieredCommission.Percentage;

                    if (currentPeriodCalculatedCommission > tieredCommission.MaxAmount)
                    {
                        currentPeriodCalculatedCommission = tieredCommission.MaxAmount.Value;
                    }

                    if (currentPeriodCalculatedCommission < tieredCommission.MinAmount)
                    {
                        currentPeriodCalculatedCommission = tieredCommission.MinAmount.Value;
                    }
                }
                else
                {
                    currentPeriodCalculatedCommission = installments.Where(p => billingDto.StartOfPeriod <= p.DueDate && p.DueDate < billingDto.EndOfPeriod).Sum(p => p.Commission) ?? 0;
                }

                if (currentPeriodCalculatedCommission > contract.PeriodMaxCommissionAmount)
                {
                    currentPeriodFinalCommission = contract.PeriodMaxCommissionAmount.Value;
                }
                else if (currentPeriodCalculatedCommission < contract.PeriodMinCommissionAmount)
                {
                    currentPeriodFinalCommission = contract.PeriodMinCommissionAmount.Value;
                }
                else
                {
                    currentPeriodFinalCommission = currentPeriodCalculatedCommission;
                }

                var alternativeBilling = alternativeBillings.LastOrDefault();

                if (alternativeBilling != null)
                {
                    previousDebitAmount = alternativeBilling.GetDebitAmount();

                    if (previousDebitAmount != 0)
                    {
                        alternativeBilling.Overdue();
                        overdueBilling = alternativeBilling;
                    }
                }

                var billing = new MerchantBilling(contract.TenantId, contract.TenantId, contract.MerchantId,
                    BillingType.TenantToMerchant, contract.BillingPeriodType, previousDebitAmount, 0, 0,
                    billingDto.StartOfPeriod, billingDto.EndOfPeriod, 0, billingDto.ContractIds, currentPeriodCalculatedCommission,
                    currentPeriodFinalCommission, refundedTransactionsCommission, previousPeriodRefundedTransactions,
                    currentPeriodPurchaseTransactions, installments, overdueBilling);

                if (billing.Amount == 0 && contract.HasEndorsement)
                {
                    continue;
                }

                billings.Add(billing);

                alternativeBillings.Add(billing);
            }
        }

        await merchantBillingRepository.AddRangeAsync(billings, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static List<NotSettledBilling> OverdueThenFilterBillings(List<NotSettledBilling> notAssignedBillings)
    {
        for (var i = notAssignedBillings.Count - 1; i >= 0; i--)
        {
            var notAssignedBilling = notAssignedBillings[i];

            if (notAssignedBilling.Billing.Status is BillingStatus.Issued or BillingStatus.PartiallyPaid)
            {
                var payableAmount = notAssignedBilling.CalculatePayableAmount();

                if (payableAmount != 0)
                {
                    notAssignedBilling.Billing.Overdue();
                }
                else
                {
                    notAssignedBillings.RemoveAt(i);
                }
            }
        }

        return notAssignedBillings;


        var notPaidBillings = notAssignedBillings.Where(p =>
            p.Billing.Status is BillingStatus.Issued or BillingStatus.PartiallyPaid);

        foreach (var notPaidBilling in notPaidBillings)
        {
            var payableAmount = notPaidBilling.CalculatePayableAmount();

            if (payableAmount != 0)
            {
                notPaidBilling.Billing.Overdue();
            }
        }

        notAssignedBillings.RemoveAll(p => p.Billing.Status is BillingStatus.Issued or BillingStatus.PartiallyPaid);
    }

    private async Task CheckPreviousBilling(FinalInstallmentQuery finalQuery, List<BillingDto> billingDtos, ContractGroup contract, DateTime startOfPeriod, DateTime endOfPeriod, CancellationToken cancellationToken)
    {
        while (true)
        {
            var pc = new PersianCalendar();

            var contractIds = contract.ContractIds;
            var billingPeriod = contract.BillingPeriod;
            var billingPeriodType = contract.BillingPeriodType;

            switch (billingPeriodType)
            {
                case TimeInterval.Day:

                    endOfPeriod = pc.AddDays(endOfPeriod, -billingPeriod);

                    startOfPeriod = pc.AddDays(startOfPeriod, -billingPeriod);

                    break;

                case TimeInterval.Week:

                    endOfPeriod = pc.AddWeeks(endOfPeriod, -1);

                    startOfPeriod = pc.AddWeeks(startOfPeriod, -1);

                    break;

                case TimeInterval.Month:

                    endOfPeriod = pc.AddMonths(endOfPeriod, -1);

                    startOfPeriod = pc.AddMonths(startOfPeriod, -1);

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (contract.EndorsementDate >= endOfPeriod) return;

            var billingQuery = merchantBillingRepository.CreateJobBillingQuery(startOfPeriod, endOfPeriod);

            var installmentQuery = merchantInstallmentRepository.CreateJobInstallmentQuery(startOfPeriod, endOfPeriod, contract.ContractIds);

            var financialDocumentQuery = financialDocumentRepository.CreateJobFinancialDocumentQuery(startOfPeriod, endOfPeriod, contract.ContractIds);

            var result = await merchantBillingRepository.FindInContractPeriodAsync(billingQuery, cancellationToken);

            if (result) return;

            var billingDto = new BillingDto
            {
                ContractGroup = contract,
                ContractIds = contractIds,
                EndOfPeriod = endOfPeriod,
                StartOfPeriod = startOfPeriod
            };

            billingDtos.Add(billingDto);

            finalQuery.BuildQueries(installmentQuery, financialDocumentQuery);
        }
    }

    private void CheckPreviousInstallments(FinalInstallmentQuery finalQuery, List<BillingDto> billingDtos, ContractGroup contract, DateTime startOfPeriod, DateTime endOfPeriod, InstallmentRange installmentRanges)
    {
        while (true)
        {
            var pc = new PersianCalendar();

            var contractIds = contract.ContractIds;
            var billingPeriod = contract.BillingPeriod;
            var billingPeriodType = contract.BillingPeriodType;

            switch (billingPeriodType)
            {
                case TimeInterval.Day:

                    endOfPeriod = pc.AddDays(endOfPeriod, -billingPeriod);

                    startOfPeriod = pc.AddDays(startOfPeriod, -billingPeriod);

                    break;

                case TimeInterval.Week:

                    endOfPeriod = pc.AddWeeks(endOfPeriod, -1);

                    startOfPeriod = pc.AddWeeks(startOfPeriod, -1);

                    break;

                case TimeInterval.Month:

                    endOfPeriod = pc.AddMonths(endOfPeriod, -1);

                    startOfPeriod = pc.AddMonths(startOfPeriod, -1);

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (installmentRanges.MinDueDate >= endOfPeriod) return;

            if (!installmentRanges.HasIntersection(startOfPeriod, endOfPeriod)) continue;

            var installmentQuery = merchantInstallmentRepository.CreateJobInstallmentQuery(startOfPeriod, endOfPeriod, contract.ContractIds);

            var financialDocumentQuery = financialDocumentRepository.CreateJobFinancialDocumentQuery(startOfPeriod, endOfPeriod, contract.ContractIds);

            var billingDto = new BillingDto
            {
                ContractGroup = contract,
                ContractIds = contractIds,
                EndOfPeriod = endOfPeriod,
                StartOfPeriod = startOfPeriod
            };

            billingDtos.Add(billingDto);

            finalQuery.BuildQueries(installmentQuery, financialDocumentQuery);
        }
    }
}

public sealed record FinalInstallmentQuery
{
    public IQueryable<MerchantInstallment> InstallmentQuery { get; set; }

    public IQueryable<FinancialDocument> FinancialDocumentQuery { get; set; }

    public void BuildQueries(IQueryable<MerchantInstallment> installmentQuery, IQueryable<FinancialDocument> financialDocumentQuery)
    {
        InstallmentQuery = InstallmentQuery == null ?
            installmentQuery : InstallmentQuery.Union(installmentQuery);

        FinancialDocumentQuery = FinancialDocumentQuery == null ?
            financialDocumentQuery : FinancialDocumentQuery.Union(financialDocumentQuery);
    }
}