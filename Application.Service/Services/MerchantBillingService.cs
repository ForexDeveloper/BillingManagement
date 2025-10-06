using System;
using System.Linq;
using System.Threading;
using Domain.Core.Enums;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;
using Application.Service.Contracts;
using Domain.Core.UnitOfWorkContracts;
using Domain.Core.Entities.BillingAggregate.Dtos;
using Domain.Core.Entities.MerchantBillingAggregate;
using Domain.Core.Entities.FinancialDocumentAggregate;
using Domain.Core.Entities.MerchantInstallmentAggregate;
using Domain.Core.Entities.FinancialDocumentAggregate.Dtos;
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
        var financialQuery = new FinancialQuery();

        var overdueBillings = await OverdueExpiredBillings(cancellationToken);

        var negativeBillings = await merchantBillingRepository.GetNegativeSettledBillings(cancellationToken);

        var allContracts = await tenantMerchantContractRepository.GetAllGroupContractAsync(cancellationToken);

        var billingGroups = await CreateBillingGroups(allContracts, financialQuery, cancellationToken);

        var installmentGroup = await merchantInstallmentRepository.GetGroupContractInstallments(
            financialQuery.InstallmentQuery, cancellationToken);

        var financialDocumentGroup = await financialDocumentRepository.GetGroupContractFinancialDocuments(
           financialQuery.FinancialDocumentQuery, cancellationToken);

        var billings = new List<MerchantBilling>();

        foreach (var billingGroup in billingGroups)
        {
            var billingDtos = billingGroup.Value;

            var oneDeactiveContractHasBilling = false;

            List<MerchantBilling> replicateBillings = [];

            for (var i = 0; i < billingDtos.Count; i++)
            {
                decimal previousDebitAmount = 0;
                decimal previousCreditAmount = 0;
                decimal previousPenaltyAmount = 0;
                decimal purchaseTransactionsAmount = 0;
                decimal refundedTransactionsAmount = 0;
                decimal purchaseTransactionsCommission = 0;
                decimal refundedTransactionsCommission = 0;
                decimal purchaseTransactionsCalculatedCommission = 0;

                var billingDto = billingDtos[i];

                var contract = billingDto.ContractGroup;

                var purchaseFinancialDocuments = new List<FinancialDocumentDto>();

                var contractIdentifier = new ContractIdentifier(contract.TenantId, contract.MerchantId, contract.BillingPeriod,
                    contract.BillingPeriodType, contract.BillingDailyOriginDate, contract.CommissionCalculationType);

                var installments = installmentGroup.GetValueOrDefault(contractIdentifier)?.Where(p =>
                    billingDto.StartOfPeriod <= p.DueDate && p.DueDate < billingDto.EndOfPeriod).ToList() ?? [];

                var financialDocuments = financialDocumentGroup.GetValueOrDefault(contractIdentifier)?.Where(p =>
                    billingDto.StartOfPeriod <= p.CreatedDateTime && p.CreatedDateTime < billingDto.EndOfPeriod).ToList() ?? [];

                foreach (var installment in installments)
                {
                    purchaseTransactionsAmount += installment.Amount;
                    purchaseTransactionsCalculatedCommission += installment.Commission;
                }

                foreach (var financialDocumentDto in financialDocuments)
                {
                    switch (financialDocumentDto.Type)
                    {
                        case FinancialDocumentType.Purchase:
                            purchaseFinancialDocuments.Add(financialDocumentDto);
                            break;

                        case FinancialDocumentType.Refund:
                            refundedTransactionsAmount += financialDocumentDto.Amount;
                            refundedTransactionsCommission += financialDocumentDto.PurchaseCommission ?? 0;
                            break;

                        case FinancialDocumentType.Reverse:
                            break;

                        default:
                            continue;
                    }
                }

                if (contract.CommissionCalculationType == CommissionCalculationType.UniformTiered)
                {
                    purchaseTransactionsCalculatedCommission = CalculateUniformedTieredCommission(contract, purchaseTransactionsAmount);
                }

                purchaseTransactionsCommission = CalculateFinalCommission(contract, purchaseTransactionsCalculatedCommission);

                MerchantBilling debtorBilling = null;

                MerchantBilling creditorBilling = null;

                var debtorBillings = GetDebtorBillings(overdueBillings, billingDto);

                var creditorBillings = GetCreditorBillings(negativeBillings, billingDto);

                if (i != 0)
                {
                    var replicateBilling = replicateBillings.Last();

                    var previousContract = billingDtos[i - 1].ContractGroup;

                    var previousContractIdentifier = new ContractIdentifier(previousContract.TenantId,
                        previousContract.MerchantId, previousContract.BillingPeriod, previousContract.BillingPeriodType,
                        previousContract.BillingDailyOriginDate, previousContract.CommissionCalculationType);

                    if (contractIdentifier == previousContractIdentifier)
                    {
                        var payableAmount = replicateBilling.GetPayableAmount();

                        switch (payableAmount)
                        {
                            case > 0:
                                replicateBilling.Overdue();
                                debtorBilling = replicateBilling;
                                previousDebitAmount = payableAmount;
                                break;

                            case < 0:
                                replicateBilling.Settle();
                                creditorBilling = replicateBilling;
                                previousCreditAmount = payableAmount;
                                break;
                        }
                    }
                    else
                    {
                        replicateBillings.Clear();

                        debtorBillings.ForEach(p => p.SetAttachment());

                        creditorBillings.ForEach(p => p.SetAttachment());

                        debtorBilling = debtorBillings.FirstOrDefault();

                        creditorBilling = creditorBillings.FirstOrDefault();

                        previousDebitAmount = debtorBillings.Sum(p => p.GetPayableAmount());

                        previousCreditAmount = creditorBillings.Sum(p => p.GetPayableAmount());
                    }
                }
                else
                {
                    debtorBillings.ForEach(p => p.SetAttachment());

                    creditorBillings.ForEach(p => p.SetAttachment());

                    debtorBilling = debtorBillings.FirstOrDefault();

                    creditorBilling = creditorBillings.FirstOrDefault();

                    previousDebitAmount = debtorBillings.Sum(p => p.GetPayableAmount());

                    previousCreditAmount = creditorBillings.Sum(p => p.GetPayableAmount());
                }

                var billing = new MerchantBilling(contract.TenantId,
                    contract.TenantId,
                    contract.MerchantId,
                    BillingType.TenantToMerchant,
                    contract.BillingPeriodType,
                    billingDto.StartOfPeriod,
                    billingDto.EndOfPeriod,
                    0,
                    billingDto.ContractIds,
                    previousDebitAmount,
                    previousCreditAmount,
                    previousPenaltyAmount,
                    purchaseTransactionsAmount,
                    refundedTransactionsAmount,
                    purchaseTransactionsCommission,
                    refundedTransactionsCommission,
                    purchaseTransactionsCalculatedCommission,
                    installments, 
                    debtorBilling,
                    creditorBilling);

                replicateBillings.Add(billing);

                if (billing.Amount == 0)
                {
                    if (!contract.Status) continue;

                    if (contract.Status && !billingDto.CurrentPeriod) continue;

                    if (contract.Status && billingDto.CurrentPeriod && oneDeactiveContractHasBilling) continue;
                }
                else
                {
                    if (!contract.Status && billingDto.CurrentPeriod)
                    {
                        oneDeactiveContractHasBilling = true;
                    }
                }

                billings.Add(billing);
            }
        }

        await merchantBillingRepository.AddRangeAsync(billings, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Dictionary<TenantMerchantIdentifier, List<BillingDto>>> CreateBillingGroups(List<ContractGroup> allContracts,
        FinancialQuery financialQuery, CancellationToken cancellationToken)
    {
        var billings = new Dictionary<TenantMerchantIdentifier, List<BillingDto>>();

        var t = allContracts.Where(p => p.Status);

        foreach (var contract in allContracts)
        {
            var billingDtos = new List<BillingDto>();

            var lastBillingDueDate = await merchantBillingRepository.GetLastBillingDueDate(contract.ContractIds, cancellationToken);

            switch (contract.Status)
            {
                case true:

                    var minInstallmentDueDate = await merchantInstallmentRepository.GetMinInstallmentDueDate(contract.ContractIds, cancellationToken);

                    ScanPeriodsForActiveContract(contract, lastBillingDueDate, minInstallmentDueDate, financialQuery, billingDtos);

                    break;

                case false:

                    var installmentRange = await merchantInstallmentRepository.GetInstallmentRanges(contract.ContractIds, cancellationToken);

                    ScanPeriodsForDeactiveContract(contract, lastBillingDueDate, installmentRange, financialQuery, billingDtos);

                    break;
            }

            var tenantMerchantId = new TenantMerchantIdentifier(contract.TenantId, contract.MerchantId);

            if (!billings.TryAdd(tenantMerchantId, billingDtos))
            {
                billings.GetValueOrDefault(tenantMerchantId).AddRange(billingDtos);
            }
        }

        return billings;
    }

    private void ScanPeriodsForActiveContract(ContractGroup contract, DateTime? lastBillingDueDate,
        DateTime? minInstallmentDueDate, FinancialQuery financialQuery, List<BillingDto> billingDtos)
    {
        DateTime endOfPeriod;
        DateTime startOfPeriod;

        var today = DateTime.Today;

        if (!lastBillingDueDate.HasValue && !minInstallmentDueDate.HasValue) return;

        if (lastBillingDueDate.HasValue)
        {
            (startOfPeriod, endOfPeriod) = GetPeriodByLastBillingDueDate(contract, lastBillingDueDate.Value);
        }
        else
        {
            (startOfPeriod, endOfPeriod) = GetPeriodByMinInstallmentDueDate(contract, minInstallmentDueDate.Value);
        }

        if (endOfPeriod > today) return;

        var currentPeriod = endOfPeriod == today;

        CreateBillingFinancialQuery(contract, financialQuery, billingDtos, currentPeriod, startOfPeriod, endOfPeriod);

        if (!currentPeriod) ScanPeriodsForActiveContract(contract, endOfPeriod, minInstallmentDueDate, financialQuery, billingDtos);
    }

    private void ScanPeriodsForDeactiveContract(ContractGroup contract, DateTime? lastBillingDueDate,
        InstallmentRange installmentRange, FinancialQuery financialQuery, List<BillingDto> billingDtos)
    {
        bool currentPeriod;
        DateTime endOfPeriod;
        DateTime startOfPeriod;

        var today = DateTime.Today;

        var endorsementDate = contract.EndorsementDate;

        if (lastBillingDueDate.HasValue && installmentRange != null)
        {
            var lastInstallmentDueDate = installmentRange.MaxDueDate;

            if ((endorsementDate < lastBillingDueDate) ||
                (lastBillingDueDate < endorsementDate && endorsementDate <= lastInstallmentDueDate))
            {
                (startOfPeriod, endOfPeriod) = GetPeriodByLastBillingDueDate(contract, lastBillingDueDate.Value);

                if (endOfPeriod > today) return;

                currentPeriod = endOfPeriod == today;

                if (endOfPeriod > lastInstallmentDueDate)
                {
                    CreateBillingFinancialQuery(contract, financialQuery, billingDtos, currentPeriod, startOfPeriod, endOfPeriod);
                }
                else
                {
                    CreateBillingFinancialQuery(contract, financialQuery, billingDtos, currentPeriod, startOfPeriod, endOfPeriod);

                    ScanPeriodsForDeactiveContract(contract, endOfPeriod, installmentRange, financialQuery, billingDtos);
                }
            }

            else if (endorsementDate > lastInstallmentDueDate)
            {
                (startOfPeriod, endOfPeriod) = GetPeriodByLastBillingDueDate(contract, lastBillingDueDate.Value);

                if (endOfPeriod > today) return;

                currentPeriod = endOfPeriod == today;

                if (endOfPeriod > endorsementDate) return;

                CreateBillingFinancialQuery(contract, financialQuery, billingDtos, currentPeriod, startOfPeriod, endOfPeriod);

                ScanPeriodsForDeactiveContract(contract, endOfPeriod, installmentRange, financialQuery, billingDtos);
            }
        }

        if (lastBillingDueDate.HasValue && installmentRange == null)
        {
            if (lastBillingDueDate > endorsementDate) return;

            (startOfPeriod, endOfPeriod) = GetPeriodByLastBillingDueDate(contract, lastBillingDueDate.Value);

            if (endOfPeriod > today) return;

            currentPeriod = endOfPeriod == today;

            if (endOfPeriod > endorsementDate) return;

            CreateBillingFinancialQuery(contract, financialQuery, billingDtos, currentPeriod, startOfPeriod, endOfPeriod);

            ScanPeriodsForDeactiveContract(contract, endOfPeriod, installmentRange, financialQuery, billingDtos);
        }

        if (!lastBillingDueDate.HasValue && installmentRange != null)
        {
            var minInstallmentDueDate = installmentRange.MinDueDate;

            var lastInstallmentDueDate = installmentRange.MaxDueDate;

            (startOfPeriod, endOfPeriod) = GetPeriodByMinInstallmentDueDate(contract, minInstallmentDueDate);

            if (endOfPeriod > today) return;

            currentPeriod = endOfPeriod == today;

            if (lastInstallmentDueDate >= endorsementDate)
            {
                if (endOfPeriod > lastInstallmentDueDate)
                {
                    CreateBillingFinancialQuery(contract, financialQuery, billingDtos, currentPeriod, endOfPeriod, startOfPeriod);
                }
                else
                {
                    CreateBillingFinancialQuery(contract, financialQuery, billingDtos, currentPeriod, startOfPeriod, endOfPeriod);

                    ScanPeriodsForDeactiveContract(contract, endOfPeriod, installmentRange, financialQuery, billingDtos);
                }
            }
            else
            {
                if (endOfPeriod > endorsementDate) return;

                CreateBillingFinancialQuery(contract, financialQuery, billingDtos, currentPeriod, startOfPeriod, endOfPeriod);

                ScanPeriodsForDeactiveContract(contract, endOfPeriod, installmentRange, financialQuery, billingDtos);
            }
        }
    }

    private void CreateBillingFinancialQuery(ContractGroup contract, FinancialQuery financialQuery,
        List<BillingDto> billingDtos, bool currentPeriod, DateTime startOfPeriod, DateTime endOfPeriod)
    {
        var billingDto = new BillingDto
        {
            CurrentPeriod = currentPeriod,
            ContractGroup = contract,
            EndOfPeriod = endOfPeriod,
            StartOfPeriod = startOfPeriod,
            ContractIds = contract.ContractIds
        };

        billingDtos.Add(billingDto);

        var installmentQuery = merchantInstallmentRepository.CreateJobInstallmentQuery(startOfPeriod, endOfPeriod, contract.ContractIds);

        var financialDocumentQuery = financialDocumentRepository.CreateJobFinancialDocumentQuery(startOfPeriod, endOfPeriod, contract.ContractIds);

        financialQuery.BuildQueries(installmentQuery, financialDocumentQuery);
    }

    private async Task<List<NotSettledBilling>> OverdueExpiredBillings(CancellationToken cancellationToken)
    {
        var notSettledBillings = await merchantBillingRepository.GetOverdueOrNotSettledBillings(cancellationToken);

        for (var i = notSettledBillings.Count - 1; i >= 0; i--)
        {
            var notAssignedBilling = notSettledBillings[i];

            if (notAssignedBilling.Billing.Status is BillingStatus.Issued or BillingStatus.PartiallyPaid)
            {
                var payableAmount = notAssignedBilling.CalculatePayableAmount();

                if (payableAmount != 0)
                {
                    notAssignedBilling.Billing.Overdue();
                }
                else
                {
                    notSettledBillings.RemoveAt(i);
                }
            }
        }

        return notSettledBillings;


        var notPaidBillings = notSettledBillings.Where(p =>
            p.Billing.Status is BillingStatus.Issued or BillingStatus.PartiallyPaid);

        foreach (var notPaidBilling in notPaidBillings)
        {
            var payableAmount = notPaidBilling.CalculatePayableAmount();

            if (payableAmount != 0)
            {
                notPaidBilling.Billing.Overdue();
            }
        }

        notSettledBillings.RemoveAll(p => p.Billing.Status is BillingStatus.Issued or BillingStatus.PartiallyPaid);
    }

    private static decimal CalculateFinalCommission(ContractGroup contract, decimal currentPeriodCalculatedCommission)
    {
        decimal currentPeriodFinalCommission;

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

        return currentPeriodFinalCommission;
    }

    private static decimal CalculateUniformedTieredCommission(ContractGroup contract, decimal totalTransactionsAmount)
    {
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

        if (tieredCommission == null) return 0;

        var currentPeriodCalculatedCommission = totalTransactionsAmount * tieredCommission.Percentage;

        if (currentPeriodCalculatedCommission > tieredCommission.MaxAmount)
        {
            currentPeriodCalculatedCommission = tieredCommission.MaxAmount.Value;
        }

        if (currentPeriodCalculatedCommission < tieredCommission.MinAmount)
        {
            currentPeriodCalculatedCommission = tieredCommission.MinAmount.Value;
        }

        return currentPeriodCalculatedCommission;
    }

    private static List<MerchantBilling> GetDebtorBillings(List<NotSettledBilling> overdueBillings, BillingDto billingDto)
    {
        var debtorBillings = overdueBillings.Where(p => billingDto.ContractIds.Any(q => p.Billing.ContractIds.Contains(q)))
            .Select(p => p.Billing).ToList();

        if (!debtorBillings.Any())
        {
            debtorBillings = overdueBillings.Where(p => billingDto.ContractIds.Contains(p.ActiveContractId))
                .Select(p => p.Billing).ToList();
        }

        return debtorBillings;
    }

    private static List<MerchantBilling> GetCreditorBillings(List<NegativeSettledBilling> negativeSettledBillings, BillingDto billingDto)
    {
        var creditorBillings = negativeSettledBillings.Where(p => billingDto.ContractIds.Any(q => p.Billing.ContractIds.Contains(q)))
            .Select(p => p.Billing).ToList();

        if (!creditorBillings.Any())
        {
            creditorBillings = negativeSettledBillings.Where(p => billingDto.ContractIds.Contains(p.ActiveContractId))
                .Select(p => p.Billing).ToList();
        }

        return creditorBillings;
    }

    private static bool ShouldSkipBilling(decimal billingAmount, bool contractStatus, bool isCurrentPeriod, ref bool oneDeactiveContractHasBilling)
    {
        if (billingAmount == 0)
        {
            if (!contractStatus) return true;

            if (!isCurrentPeriod) return true;

            if (oneDeactiveContractHasBilling) return true;
        }
        else
        {
            if (!contractStatus && isCurrentPeriod)
            {
                oneDeactiveContractHasBilling = true;
            }
        }

        return false;
    }

    private static (DateTime StartOfPeriod, DateTime EndOfPeriod) GetPeriodByLastBillingDueDate(ContractGroup contract, DateTime lastBillingDueDate)
    {
        DateTime startOfPeriod;
        DateTime endOfPeriod;

        var pc = new PersianCalendar();

        switch (contract.BillingPeriodType)
        {
            case TimeInterval.Day:

                startOfPeriod = lastBillingDueDate;

                endOfPeriod = pc.AddDays(startOfPeriod, contract.BillingPeriod);

                break;

            case TimeInterval.Week:

                startOfPeriod = lastBillingDueDate;

                endOfPeriod = pc.AddWeeks(startOfPeriod, 1);

                break;

            case TimeInterval.Month:

                startOfPeriod = lastBillingDueDate;

                endOfPeriod = pc.AddMonths(startOfPeriod, 1);

                endOfPeriod = AdjustEndOfPeriod(pc, endOfPeriod, contract.BillingPeriod);

                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        return new ValueTuple<DateTime, DateTime>(startOfPeriod, endOfPeriod);
    }

    private static (DateTime StartOfPeriod, DateTime EndOfPeriod) GetPeriodByMinInstallmentDueDate(ContractGroup contract, DateTime minInstallmentDueDate)
    {
        DateTime startOfPeriod;
        DateTime endOfPeriod;

        var pc = new PersianCalendar();

        var year = pc.GetYear(minInstallmentDueDate);
        var month = pc.GetMonth(minInstallmentDueDate);
        var dayOfWeek = pc.GetDayOfWeek(minInstallmentDueDate);
        var dayOfMonth = pc.GetDayOfMonth(minInstallmentDueDate);

        var period = contract.BillingPeriod;

        switch (contract.BillingPeriodType)
        {
            case TimeInterval.Day:

                if (!contract.BillingDailyOriginDate.HasValue) throw new NullReferenceException();

                var originDate = contract.BillingDailyOriginDate.Value;

                if (minInstallmentDueDate.Date < originDate.Date) throw new NullReferenceException();

                var totalDays = (minInstallmentDueDate.Date - originDate.Date).Days;

                var difference = period - (totalDays % period);

                startOfPeriod = pc.AddDays(minInstallmentDueDate, -difference);

                endOfPeriod = pc.AddDays(startOfPeriod, period);

                break;

            case TimeInterval.Week:

                if ((DayOfWeek)period >= dayOfWeek)
                {
                    startOfPeriod = pc.AddWeeks(new DateTime(year, month, period, pc), -1);
                }
                else
                {
                    startOfPeriod = pc.ToDateTime(year, month, period, 0, 0, 0, 0);
                }

                endOfPeriod = pc.AddWeeks(startOfPeriod, 1);

                break;

            case TimeInterval.Month:

                if (period >= dayOfMonth)
                {
                    startOfPeriod = pc.AddMonths(new DateTime(year, month, period, pc), -1);
                }
                else
                {
                    startOfPeriod = pc.ToDateTime(year, month, period, 0, 0, 0, 0);
                }

                endOfPeriod = pc.AddMonths(startOfPeriod, 1);

                endOfPeriod = AdjustEndOfPeriod(pc, endOfPeriod, contract.BillingPeriod);

                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        return new ValueTuple<DateTime, DateTime>(startOfPeriod, endOfPeriod);
    }

    private static DateTime AdjustEndOfPeriod(PersianCalendar pc, DateTime endOfPeriod, int billingPeriod)
    {
        var year = pc.GetYear(endOfPeriod);
        var month = pc.GetMonth(endOfPeriod);
        var dayOfMonth = pc.GetDayOfMonth(endOfPeriod);

        if (dayOfMonth is 29 or 30 && (billingPeriod is 30 or 31))
        {
            endOfPeriod = pc.ToDateTime(year, month, billingPeriod, 0, 0, 0, 0);
        }

        return endOfPeriod;
    }
}

public sealed record FinancialQuery
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