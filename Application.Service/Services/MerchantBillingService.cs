using System;
using System.Linq;
using System.Threading;
using Domain.Core.Enums;
using System.Threading.Tasks;
using Application.Service.Helper;
using System.Collections.Generic;
using Application.Service.Contracts;
using Application.Service.Dtos.Shared;
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
    public async Task IssueOrOverdueBilling(DateTime jobCreatedDateTime, CancellationToken cancellationToken)
    {
        var overdueBillings = await OverdueExpiredBillings(cancellationToken);

        var negativeBillings = await merchantBillingRepository.GetNegativeSettledBillings(cancellationToken);

        var groupContracts = await tenantMerchantContractRepository.GetAllGroupContractAsync(cancellationToken);

        await CreateMerchantBillingsAtomic(groupContracts, overdueBillings, negativeBillings, jobCreatedDateTime, cancellationToken);
    }

    private async Task CreateMerchantBillings(List<ContractGroup> contracts, List<NotSettledBilling> overdueBillings,
        List<NegativeSettledBilling> negativeBillings, DateTime jobCreatedDateTime, CancellationToken cancellationToken)
    {
        var activeContracts = contracts.Where(p => p.Status).ToList();

        var deactiveContracts = contracts.Where(p => !p.Status).ToList();

        foreach (var groupContracts in contracts.GroupBy(p => new TenantMerchantIdentifier(p.TenantId, p.MerchantId)))
        {
            foreach (var contract in groupContracts)
            {
                var billingDtos = new List<BillingDto>();

                var financialQuery = new FinancialQuery();

                var billings = new List<MerchantBilling>();

                var replicateBillings = new List<MerchantBilling>();

                var lastBillingDueDate = await merchantBillingRepository.GetLastBillingDueDate(contract.ContractIds, cancellationToken);

                var installmentRange = await merchantInstallmentRepository.GetInstallmentRange(contract.ContractIds, lastBillingDueDate, cancellationToken);

                var financialDocumentRange = await financialDocumentRepository.GetFinancialDocumentRange(contract.ContractIds, lastBillingDueDate, jobCreatedDateTime, cancellationToken);

                var financialDataRange = FinancialDataRange.Create(installmentRange, financialDocumentRange);

                if (financialDataRange != null)
                {

                }

                switch (contract.Status)
                {
                    case true:
                        await ScanPeriodsForActiveContract(contract, lastBillingDueDate, financialDataRange,
                            financialQuery, billingDtos, cancellationToken);
                        break;

                    case false:
                        ScanPeriodsForDeactiveContract(contract, lastBillingDueDate, financialDataRange, financialQuery,
                            billingDtos);
                        break;
                }

                var oneDeactiveContractHasBilling = false;

                var debtorBillings = GetDebtorBillings(overdueBillings, contract.ContractIds);

                var creditorBillings = GetCreditorBillings(negativeBillings, contract.ContractIds);

                for (var i = 0; i < billingDtos.Count; i++)
                {
                    decimal previousDebitAmount = 0;
                    decimal previousCreditAmount = 0;
                    decimal previousPenaltyAmount = 0;
                    decimal purchaseTransactionsAmount = 0;
                    decimal purchaseTransactionsCashAmount = 0;
                    decimal purchaseTransactionsCreditAmount = 0;
                    decimal purchaseTransactionsPrepaymentAmount = 0;
                    decimal refundedTransactionsAmount = 0;
                    decimal purchaseTransactionsCommission = 0;
                    decimal refundedTransactionsCommission = 0;
                    decimal purchaseTransactionsCalculatedCommission = 0;

                    var billingDto = billingDtos[i];

                    var installments = await merchantInstallmentRepository.GetInstallmentsInSpecificPeriod(contract.ContractIds,
                        billingDto.StartOfPeriod, billingDto.EndOfPeriod, cancellationToken);

                    var financialDocuments = await financialDocumentRepository.GetFinancialDocumentsInSpecificPeriod(contract.ContractIds,
                        billingDto.StartOfPeriod, billingDto.EndOfPeriod, cancellationToken);

                    foreach (var installment in installments)
                    {
                        purchaseTransactionsAmount += installment.Amount;
                        purchaseTransactionsCashAmount += installment.CashAmount;
                        purchaseTransactionsCreditAmount += installment.CreditAmount;
                        purchaseTransactionsPrepaymentAmount += installment.PrepaymentAmount;
                        purchaseTransactionsCalculatedCommission += installment.Commission;
                    }

                    foreach (var financialDocumentDto in financialDocuments)
                    {
                        refundedTransactionsAmount += financialDocumentDto.Amount;
                        refundedTransactionsCommission += financialDocumentDto.PurchaseCommission ?? 0;
                    }

                    if (contract.CommissionCalculationType == CommissionCalculationType.UniformTiered)
                    {
                        var sumOfTieredTransactions = purchaseTransactionsAmount;

                        if (contract.CommissionReferenceTypes != null && contract.CommissionReferenceTypes.Any())
                        {
                            sumOfTieredTransactions = 0;

                            foreach (var commissionReferenceType in contract.CommissionReferenceTypes)
                            {
                                switch (commissionReferenceType)
                                {
                                    case CommissionReferenceType.CashAmount:
                                        sumOfTieredTransactions += purchaseTransactionsCashAmount;
                                        break;

                                    case CommissionReferenceType.CreditAmount:
                                        sumOfTieredTransactions += purchaseTransactionsCreditAmount;
                                        break;

                                    case CommissionReferenceType.PrepaymentAmount:
                                        sumOfTieredTransactions += purchaseTransactionsPrepaymentAmount;
                                        break;

                                    case CommissionReferenceType.InterestAmount:
                                        break;

                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }
                            }
                        }

                        purchaseTransactionsCalculatedCommission = CalculateUniformedTieredCommission(contract, sumOfTieredTransactions);
                    }

                    purchaseTransactionsCommission = CalculateFinalCommission(contract, purchaseTransactionsCalculatedCommission);

                    MerchantBilling debtorBilling = null;

                    MerchantBilling creditorBilling = null;

                    if (i > 0)
                    {
                        var replicateBilling = replicateBillings.Last();

                        var previousContract = billingDtos[i - 1].ContractGroup;

                        var contractIdentifier = contract.CreateIdentifier();

                        var previousContractIdentifier = previousContract.CreateIdentifier();

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
                        }
                    }

                    if (replicateBillings.Count == 0)
                    {
                        debtorBillings.ForEach(p => p.Transfer());

                        creditorBillings.ForEach(p => p.Transfer());

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
                        contract.MainContractId,
                        contract.ContractIds,
                        previousDebitAmount,
                        previousCreditAmount,
                        previousPenaltyAmount,
                        purchaseTransactionsAmount,
                        refundedTransactionsAmount,
                        purchaseTransactionsCommission,
                        refundedTransactionsCommission,
                        purchaseTransactionsCalculatedCommission,
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

                await merchantBillingRepository.AddRangeAsync(billings, cancellationToken);

                //await unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
    }

    private async Task CreateMerchantBillingsAtomic(List<ContractGroup> contracts, List<NotSettledBilling> overdueBillings,
    List<NegativeSettledBilling> negativeBillings, DateTime jobCreatedDateTime, CancellationToken cancellationToken)
    {
        var activeContracts = contracts.Where(p => p.Status).ToList();

        foreach (var groupContracts in contracts.GroupBy(p => new TenantMerchantIdentifier(p.TenantId, p.MerchantId)))
        {
            foreach (var contract in groupContracts)
            {
                var billingDtos = new List<BillingDto>();

                var financialQuery = new FinancialQuery();

                var billings = new List<MerchantBilling>();

                var replicateBillings = new List<MerchantBilling>();

                var lastBillingDueDate = await merchantBillingRepository.GetLastBillingDueDate(contract.ContractIds, cancellationToken);

                var installmentRange = await merchantInstallmentRepository.GetInstallmentRange(contract.ContractIds, lastBillingDueDate, cancellationToken);

                var financialDocumentRange = await financialDocumentRepository.GetFinancialDocumentRange(contract.ContractIds, lastBillingDueDate, jobCreatedDateTime, cancellationToken);

                var financialDataRange = FinancialDataRange.Create(installmentRange, financialDocumentRange);

                if (financialDataRange != null)
                {

                }

                switch (contract.Status)
                {
                    case true:
                        await ScanPeriodsForActiveContract(contract, lastBillingDueDate, financialDataRange,
                            financialQuery, billingDtos, cancellationToken);
                        break;

                    case false:
                        ScanPeriodsForDeactiveContract(contract, lastBillingDueDate, financialDataRange, financialQuery,
                            billingDtos);
                        break;
                }

                var oneDeactiveContractHasBilling = false;

                var debtorBillings = GetDebtorBillings(overdueBillings, contract.ContractIds);

                var creditorBillings = GetCreditorBillings(negativeBillings, contract.ContractIds);

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

                    purchaseTransactionsAmount = await merchantInstallmentRepository.GetSumOfTransactionsInSpecificPeriod(contract.ContractIds,
                            billingDto.StartOfPeriod, billingDto.EndOfPeriod, cancellationToken);

                    purchaseTransactionsCalculatedCommission = await merchantInstallmentRepository.GetSumOfCommissionsInSpecificPeriod(contract.ContractIds,
                            billingDto.StartOfPeriod, billingDto.EndOfPeriod, cancellationToken);

                    refundedTransactionsAmount = await financialDocumentRepository.GetSumOfRefundTransactionsInSpecificPeriod(contract.ContractIds,
                            billingDto.StartOfPeriod, billingDto.EndOfPeriod, cancellationToken);

                    refundedTransactionsCommission = await financialDocumentRepository.GetSumOfRefundCommissionsInSpecificPeriod(contract.ContractIds,
                            billingDto.StartOfPeriod, billingDto.EndOfPeriod, cancellationToken);

                    if (contract.CommissionCalculationType == CommissionCalculationType.UniformTiered)
                    {
                        var sumOfTieredTransactions = await merchantInstallmentRepository.GetSumOfTieredTransactionsInSpecificPeriod(contract,
                                billingDto.StartOfPeriod, billingDto.EndOfPeriod, cancellationToken);

                        purchaseTransactionsCalculatedCommission = CalculateUniformedTieredCommission(contract, sumOfTieredTransactions);
                    }

                    purchaseTransactionsCommission = CalculateFinalCommission(contract, purchaseTransactionsCalculatedCommission);

                    MerchantBilling debtorBilling = null;

                    MerchantBilling creditorBilling = null;

                    var replicateBilling = replicateBillings.LastOrDefault();

                    if (replicateBilling != null)
                    {
                        var previousContract = billingDtos[i - 1].ContractGroup;

                        var contractIdentifier = contract.CreateIdentifier();

                        var previousContractIdentifier = previousContract.CreateIdentifier();

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
                        }
                    }

                    if (replicateBillings.Count == 0)
                    {
                        debtorBillings.ForEach(p => p.Transfer());

                        creditorBillings.ForEach(p => p.Transfer());

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
                        contract.MainContractId,
                        contract.ContractIds,
                        previousDebitAmount,
                        previousCreditAmount,
                        previousPenaltyAmount,
                        purchaseTransactionsAmount,
                        refundedTransactionsAmount,
                        purchaseTransactionsCommission,
                        refundedTransactionsCommission,
                        purchaseTransactionsCalculatedCommission,
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

                await merchantBillingRepository.AddRangeAsync(billings, cancellationToken);

                //await unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
    }

    private async Task ScanPeriodsForActiveContract(ContractGroup contract, DateTime? lastBillingDueDate,
        FinancialDataRange financialDataRange, FinancialQuery financialQuery, List<BillingDto> billingDtos,
        CancellationToken cancellationToken)
    {
        bool currentPeriod;

        DateTime endOfPeriod;
        DateTime startOfPeriod;

        var today = DateTime.Today;

        if (lastBillingDueDate.HasValue)
        {
            (startOfPeriod, endOfPeriod) = ContractPeriodHelper.GetPeriodByLastBillingDueDate(contract.BillingPeriod,
                contract.BillingPeriodType, contract.DailyBillingOriginDate, lastBillingDueDate.Value);

            if (endOfPeriod > today) return;

            currentPeriod = endOfPeriod == today;

            var anotherBillingFound = await merchantBillingRepository.FindAnotherBillingOnEndOfPeriod(contract.TenantId,
                contract.MerchantId, endOfPeriod, cancellationToken);

            if (!anotherBillingFound)
            {
                CreateBillingFinancialQuery(contract, financialQuery, billingDtos, currentPeriod, startOfPeriod, endOfPeriod);
            }

            await ScanPeriodsForActiveContract(contract, endOfPeriod, financialDataRange, financialQuery, billingDtos, cancellationToken);
        }

        else if (financialDataRange != null)
        {
            (startOfPeriod, endOfPeriod) = ContractPeriodHelper.GetPeriodBySpecificDate(contract.BillingPeriod,
                contract.BillingPeriodType, contract.DailyBillingOriginDate, financialDataRange.MinDate);

            if (endOfPeriod > today) return;

            currentPeriod = endOfPeriod == today;

            CreateBillingFinancialQuery(contract, financialQuery, billingDtos, currentPeriod, startOfPeriod, endOfPeriod);

            await ScanPeriodsForActiveContract(contract, endOfPeriod, financialDataRange, financialQuery, billingDtos, cancellationToken);
        }

        if (!lastBillingDueDate.HasValue && financialDataRange == null)
        {
            (startOfPeriod, endOfPeriod) = ContractPeriodHelper.GetPeriodBySpecificDate(contract.BillingPeriod,
                contract.BillingPeriodType, contract.DailyBillingOriginDate, today);

            currentPeriod = endOfPeriod == today;

            if (currentPeriod)
            {
                CreateBillingFinancialQuery(contract, financialQuery, billingDtos, true, startOfPeriod, endOfPeriod);
            }
        }
    }

    private void ScanPeriodsForDeactiveContract(ContractGroup contract, DateTime? lastBillingDueDate,
        FinancialDataRange financialDataRange, FinancialQuery financialQuery, List<BillingDto> billingDtos)
    {
        DateTime endOfPeriod;
        DateTime startOfPeriod;

        var today = DateTime.Today;

        if (financialDataRange == null) return;

        if (lastBillingDueDate.HasValue)
        {
            (startOfPeriod, endOfPeriod) = ContractPeriodHelper.GetPeriodByLastBillingDueDate(contract.BillingPeriod,
                contract.BillingPeriodType, contract.DailyBillingOriginDate, lastBillingDueDate.Value);
        }
        else
        {
            (startOfPeriod, endOfPeriod) = ContractPeriodHelper.GetPeriodBySpecificDate(contract.BillingPeriod,
                contract.BillingPeriodType, contract.DailyBillingOriginDate, financialDataRange.MinDate);
        }

        if (endOfPeriod > today) return;

        var currentPeriod = endOfPeriod == today;

        if (endOfPeriod > financialDataRange.MaxDate)
        {
            CreateBillingFinancialQuery(contract, financialQuery, billingDtos, currentPeriod, endOfPeriod, startOfPeriod);
        }
        else
        {
            CreateBillingFinancialQuery(contract, financialQuery, billingDtos, currentPeriod, startOfPeriod, endOfPeriod);

            ScanPeriodsForDeactiveContract(contract, endOfPeriod, financialDataRange, financialQuery, billingDtos);
        }
    }

    private void CreateBillingFinancialQuery(ContractGroup contract, FinancialQuery financialQuery,
        List<BillingDto> billingDtos, bool currentPeriod, DateTime startOfPeriod, DateTime endOfPeriod)
    {
        var billingDto = new BillingDto
        {
            ContractGroup = contract,
            EndOfPeriod = endOfPeriod,
            StartOfPeriod = startOfPeriod,
            CurrentPeriod = currentPeriod
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
        if (contract.TieredCommissions == null) return 0;

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

    private static List<MerchantBilling> GetDebtorBillings(List<NotSettledBilling> overdueBillings, IEnumerable<int> contactIds)
    {
        var debtorBillings = overdueBillings.Where(p => contactIds.Any(q => p.Billing.ContractIds.Contains(q)))
            .Select(p => p.Billing).ToList();

        if (!debtorBillings.Any())
        {
            debtorBillings = overdueBillings.Where(p => contactIds.Contains(p.ActiveContractId))
                .Select(p => p.Billing).ToList();
        }

        return debtorBillings;
    }

    private static List<MerchantBilling> GetCreditorBillings(List<NegativeSettledBilling> negativeSettledBillings, IEnumerable<int> contactIds)
    {
        var creditorBillings = negativeSettledBillings.Where(p => contactIds.Any(q => p.Billing.ContractIds.Contains(q)))
            .Select(p => p.Billing).ToList();

        if (!creditorBillings.Any())
        {
            creditorBillings = negativeSettledBillings.Where(p => contactIds.Contains(p.ActiveContractId))
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