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
using Domain.Core.Entities.MerchantBillingAggregate;
using Domain.Core.Entities.B2bBillingAggregate.Dtos;
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
    public async Task CreateMerchantBilling(CancellationToken cancellationToken)
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

        var notAssignedBillings = await merchantBillingRepository.GetNotAssignedBillings(cancellationToken);

        var allContracts = await tenantMerchantContractRepository.GetAllGroupContractAsync(cancellationToken);

        foreach (var contract in allContracts)
        {
            int difference;
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

                    difference = totalDays % period;

                    endOfPeriod = pc.AddDays(new DateTime(year, month, day, pc), -difference);

                    startOfPeriod = pc.AddDays(endOfPeriod, -period);

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

                    endOfPeriod = pc.AddDays(new DateTime(year, month, day, pc), -difference);

                    startOfPeriod = pc.AddWeeks(endOfPeriod, -1);

                    break;

                case TimeInterval.Month:

                    period = DateHelper.RegulateBillingPeriod(year, month, period);

                    difference = day - period;

                    if (difference >= 0)
                    {
                        endOfPeriod = pc.ToDateTime(year, month, period, 0, 0, 0, 0);
                    }
                    else
                    {
                        endOfPeriod = pc.AddMonths(new DateTime(year, month, period, pc), -1);
                    }

                    startOfPeriod = pc.AddMonths(endOfPeriod, -1);

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            var billingQuery = merchantBillingRepository.CreateJobBillingQuery(startOfPeriod, endOfPeriod);

            var installmentQuery = merchantInstallmentRepository.CreateJobInstallmentQuery(startOfPeriod, endOfPeriod, contract.ContractIds);

            var financialDocumentQuery = financialDocumentRepository.CreateJobFinancialDocumentQuery(startOfPeriod, endOfPeriod, contract.ContractIds);

            if (contract.HasEndorsement)
            {
                var installmentDates = await merchantInstallmentRepository.GetInstallmentsDates(endOfPeriod, contract.ContractIds, cancellationToken);

                if (installmentDates == null) continue;

                if (installmentDates.HasIntersection(startOfPeriod, endOfPeriod))
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

                CheckPreviousInstallments(finalQuery, billingDtos, contract, startOfPeriod, endOfPeriod, installmentDates);
            }
            else
            {
                if (difference != 0)
                {
                    if (contract.EndorsementDate >= endOfPeriod) continue;

                    var result = await merchantBillingRepository.FindInContractPeriodAsync(billingQuery, cancellationToken);

                    if (result) continue;
                }

                var billingDto = new BillingDto
                {
                    ContractGroup = contract,
                    EndOfPeriod = endOfPeriod,
                    StartOfPeriod = startOfPeriod,
                    ContractIds = contract.ContractIds
                };

                billingDtos.Add(billingDto);

                finalQuery.BuildQueries(installmentQuery, financialDocumentQuery);

                await CheckPreviousBilling(finalQuery, billingDtos, contract, startOfPeriod, endOfPeriod, cancellationToken);
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
            var index = 0;

            decimal refundedTransactionsCommission = 0;

            List<MerchantBilling> alternativeBillings = [];

            var contractIds = billingGroup.First().ContractIds;

            var overdueBilling = overdueBillings.FirstOrDefault(p => contractIds.Any(q => p.Billing.ContractIds.Contains(q)))?.Billing ??
                                 overdueBillings.FirstOrDefault(p => contractIds.Contains(p.FinalEndorsementContractId))?.Billing;

            foreach (var billingDto in billingGroup.OrderBy(p => p.EndOfPeriod))
            {
                decimal previousDebitAmount = 0;

                var contract = billingDto.ContractGroup;

                var billingKey = new ContractIdentifier(contract.TenantId, contract.MerchantId, contract.BillingPeriod,
                    contract.BillingPeriodType, contract.BillingDailyOriginDate);

                var installments = installmentGroup.GetValueOrDefault(billingKey);

                var financialDocuments = financialDocumentGroup.GetValueOrDefault(billingKey);

                var currentPeriodFinalCommission = installments.Where(p => billingDto.StartOfPeriod <= p.DueDate && p.DueDate < billingDto.EndOfPeriod).Sum(p => p.Commission);

                var currentPeriodPurchaseTransactions = installments.Where(p => billingDto.StartOfPeriod <= p.DueDate && p.DueDate < billingDto.EndOfPeriod).Sum(p => p.Amount);

                var previousPeriodRefundedTransactions = financialDocuments.Where(p => billingDto.StartOfPeriod <= p.CreatedDateTime && p.CreatedDateTime < billingDto.EndOfPeriod).Where(p => p.Type == FinancialDocumentType.Refund).Sum(p => p.Amount);

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

                index++;

                var billing = new MerchantBilling(contract.TenantId, contract.TenantId, contract.MerchantId,
                    BillingType.TenantToMerchant, contract.BillingPeriodType, previousDebitAmount, 0, 0,
                    billingDto.StartOfPeriod, billingDto.EndOfPeriod, 0, billingDto.ContractIds,
                    currentPeriodFinalCommission, refundedTransactionsCommission, previousPeriodRefundedTransactions,
                    currentPeriodPurchaseTransactions, installments, overdueBilling);

                if (billing.Amount == 0 && contract.HasEndorsement)
                {
                    continue;
                }

                if (contract.HasEndorsement && index == billingGroup.Count - 1)
                {

                }

                billings.Add(billing);

                alternativeBillings.Add(billing);
            }
        }

        await merchantBillingRepository.AddRangeAsync(billings, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task CreateMerchantBillingV2(CancellationToken cancellationToken)
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

        var notSettledBillings = await merchantBillingRepository.GetNotAssignedBillings(cancellationToken);

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
                var installmentResult = await merchantInstallmentRepository.FindInContractPeriodAsync(installmentQuery, cancellationToken);

                if (!installmentResult) continue;

                var billingDto = new BillingDto
                {
                    ContractGroup = contract,
                    EndOfPeriod = billingEndDate,
                    StartOfPeriod = billingStartDate,
                    ContractIds = contract.ContractIds
                };

                billingDtos.Add(billingDto);

                finalQuery.BuildQueries(installmentQuery, financialDocumentQuery);

                CheckPreviousInstallments(finalQuery, billingDtos, contract, billingStartDate, billingEndDate, new InstallmentDates(DateTime.Today, DateTime.Today));
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

        var financialDocumentGroup =
            await financialDocumentRepository.GetGroupContractFinancialDocuments(
                finalQuery.FinancialDocumentQuery, cancellationToken);

        var installmentGroup =
            await merchantInstallmentRepository.GetGroupContractInstallments(finalQuery.InstallmentQuery,
                cancellationToken);

        var billings = new List<MerchantBilling>();

        foreach (var billingGroup in billingGroups)
        {
            decimal refundedTransactionsCommission = 0;

            List<MerchantBilling> alternativeBillings = [];

            var contractIds = billingGroup.First().ContractIds;

            var notSettledBilling = notSettledBillings.FirstOrDefault(p => p.Billing.ContractIds.Any(q => contractIds.Contains(q)));

            foreach (var billingDto in billingGroup.OrderBy(p => p.EndOfPeriod))
            {
                decimal previousDebitAmount = 0;

                var contract = billingDto.ContractGroup;

                var endOfPeriod = billingDto.EndOfPeriod;

                var startOdPeriod = billingDto.StartOfPeriod;

                var billingKey = new ContractIdentifier(contract.TenantId, contract.MerchantId, contract.BillingPeriod,
                    contract.BillingPeriodType, contract.BillingDailyOriginDate);

                var installments = installmentGroup.GetValueOrDefault(billingKey);

                var financialDocuments = financialDocumentGroup.GetValueOrDefault(billingKey);

                var currentPeriodFinalCommission = installments.Where(p => startOdPeriod <= p.DueDate && p.DueDate < endOfPeriod).Sum(p => p.Commission);

                var currentPeriodPurchaseTransactions = installments.Where(p => startOdPeriod <= p.DueDate && p.DueDate < endOfPeriod).Sum(p => p.Amount);

                var previousPeriodRefundedTransactions = financialDocuments.Where(p => startOdPeriod <= p.CreatedDateTime && p.CreatedDateTime < endOfPeriod).Where(p => p.Type == FinancialDocumentType.Refund).Sum(p => p.Amount);

                MerchantBilling overdueBilling = null;

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
                else
                {
                    if (notSettledBilling != null)
                    {
                        previousDebitAmount = notSettledBilling.CalculatePayableAmount();

                        if (previousDebitAmount != 0)
                        {
                            notSettledBilling.Billing.Overdue();
                            overdueBilling = notSettledBilling.Billing;
                        }
                    }
                }

                var billing = new MerchantBilling(contract.TenantId, contract.TenantId, contract.MerchantId,
                    BillingType.TenantToMerchant, contract.BillingPeriodType, previousDebitAmount, 0, 0,
                    billingDto.StartOfPeriod, billingDto.EndOfPeriod, 0, billingDto.ContractIds,
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

    private void CheckPreviousInstallments(FinalInstallmentQuery finalQuery, List<BillingDto> billingDtos, ContractGroup contract, DateTime startOfPeriod, DateTime endOfPeriod, InstallmentDates installmentDates)
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

            if (installmentDates.MinDueDate >= endOfPeriod) return;

            if (!installmentDates.HasIntersection(startOfPeriod, endOfPeriod)) continue;

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