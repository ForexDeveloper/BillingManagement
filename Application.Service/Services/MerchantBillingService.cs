using System;
using System.Linq;
using System.Threading;
using Domain.Core.Enums;
using System.Globalization;
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
using Domain.Core.Entities.FinancialDocumentAggregate.Dtos;
using Domain.Core.Entities.TenantMerchantContractAggregate.Dtos;
using Domain.Core.Entities.BillingAggregate;

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
        var overdueBillings = await OverdueExpiredBillings(cancellationToken);

        var negativeBillings = await merchantBillingRepository.GetNegativeSettledBillings(cancellationToken);

        var groupContracts = await tenantMerchantContractRepository.GetAllGroupContractAsync(cancellationToken);

        await CreateMerchantBillings(groupContracts, overdueBillings, negativeBillings, cancellationToken);
    }

    private async Task CreateMerchantBillings(List<ContractGroup> contracts, List<NotSettledBilling> overdueBillings,
        List<NegativeSettledBilling> negativeBillings, CancellationToken cancellationToken)
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

                var financialDocumentRange = await financialDocumentRepository.GetFinancialDocumentRange(contract.ContractIds, lastBillingDueDate, cancellationToken);

                var financialDataRange = FinancialDataRange.Create(installmentRange, financialDocumentRange);

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

                var groupInstallments = await merchantInstallmentRepository.GetGroupContractInstallments(financialQuery.InstallmentQuery, cancellationToken);

                var groupFinancialDocuments = await financialDocumentRepository.GetGroupContractFinancialDocuments(financialQuery.FinancialDocumentQuery, cancellationToken);

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

                    var contractIdentifier = new ContractIdentifier(contract.TenantId, contract.MerchantId, contract.BillingPeriod,
                        contract.BillingPeriodType, contract.DailyBillingOriginDate, contract.CommissionCalculationType);

                    var installments = groupInstallments.GetValueOrDefault(contractIdentifier)?.Where(p =>
                        billingDto.StartOfPeriod <= p.DueDate && p.DueDate < billingDto.EndOfPeriod).ToList() ?? [];

                    var financialDocuments = groupFinancialDocuments.GetValueOrDefault(contractIdentifier)?.Where(p =>
                            billingDto.StartOfPeriod <= p.CreatedDateTime && p.CreatedDateTime < billingDto.EndOfPeriod).ToList() ?? [];

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

                    if (i != 0)
                    {
                        var replicateBilling = replicateBillings.Last();

                        var previousContract = billingDtos[i - 1].ContractGroup;

                        var previousContractIdentifier = new ContractIdentifier(previousContract.TenantId,
                            previousContract.MerchantId, previousContract.BillingPeriod, previousContract.BillingPeriodType,
                            previousContract.DailyBillingOriginDate, previousContract.CommissionCalculationType);

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

                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
    }

    private async Task CreateMerchantBillingsNormal(List<ContractGroup> contracts, List<NotSettledBilling> overdueBillings,
        List<NegativeSettledBilling> negativeBillings, CancellationToken cancellationToken)
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

                var financialDocumentRange = await financialDocumentRepository.GetFinancialDocumentRange(contract.ContractIds, lastBillingDueDate, cancellationToken);

                var financialDataRange = FinancialDataRange.Create(installmentRange, financialDocumentRange);

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

                    var contractIdentifier = new ContractIdentifier(contract.TenantId, contract.MerchantId, contract.BillingPeriod,
                        contract.BillingPeriodType, contract.DailyBillingOriginDate, contract.CommissionCalculationType);

                    var installments = await merchantInstallmentRepository.GetInstallmentsInSpecificPeriod(contract,
                        billingDto.StartOfPeriod, billingDto.EndOfPeriod, cancellationToken);

                    var financialDocuments = await financialDocumentRepository.GetFinancialDocumentsInSpecificPeriod(contract,
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

                    if (i != 0)
                    {
                        var replicateBilling = replicateBillings.Last();

                        var previousContract = billingDtos[i - 1].ContractGroup;

                        var previousContractIdentifier = new ContractIdentifier(previousContract.TenantId,
                            previousContract.MerchantId, previousContract.BillingPeriod, previousContract.BillingPeriodType,
                            previousContract.DailyBillingOriginDate, previousContract.CommissionCalculationType);

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

                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
    }

    private async Task CreateMerchantBillingsDeep(List<ContractGroup> contracts, List<NotSettledBilling> overdueBillings,
    List<NegativeSettledBilling> negativeBillings, CancellationToken cancellationToken)
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

                var financialDocumentRange = await financialDocumentRepository.GetFinancialDocumentRange(contract.ContractIds, lastBillingDueDate, cancellationToken);

                var financialDataRange = FinancialDataRange.Create(installmentRange, financialDocumentRange);

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

                    var contractIdentifier = new ContractIdentifier(contract.TenantId, contract.MerchantId, contract.BillingPeriod,
                        contract.BillingPeriodType, contract.DailyBillingOriginDate, contract.CommissionCalculationType);

                    purchaseTransactionsAmount = await merchantInstallmentRepository.GetSumOfTransactionsInSpecificPeriod(contract,
                            billingDto.StartOfPeriod, billingDto.EndOfPeriod, cancellationToken);

                    purchaseTransactionsCalculatedCommission = await merchantInstallmentRepository.GetSumOfCommissionsInSpecificPeriod(contract,
                            billingDto.StartOfPeriod, billingDto.EndOfPeriod, cancellationToken);

                    refundedTransactionsAmount = await financialDocumentRepository.GetSumOfRefundTransactionsInSpecificPeriod(contract,
                            billingDto.StartOfPeriod, billingDto.EndOfPeriod, cancellationToken);

                    refundedTransactionsCommission = await financialDocumentRepository.GetSumOfRefundCommissionsInSpecificPeriod(contract,
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

                    if (i != 0)
                    {
                        var replicateBilling = replicateBillings.Last();

                        var previousContract = billingDtos[i - 1].ContractGroup;

                        var previousContractIdentifier = new ContractIdentifier(previousContract.TenantId,
                            previousContract.MerchantId, previousContract.BillingPeriod, previousContract.BillingPeriodType,
                            previousContract.DailyBillingOriginDate, previousContract.CommissionCalculationType);

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

                await unitOfWork.SaveChangesAsync(cancellationToken);
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
            (startOfPeriod, endOfPeriod) = GetPeriodByLastBillingDueDate(contract, lastBillingDueDate.Value);

            if (endOfPeriod > today) return;

            currentPeriod = endOfPeriod == today;

            var anotherBillingFound = await merchantBillingRepository.FindAnotherBillingOnEndOfPeriod(endOfPeriod, cancellationToken);

            if (!anotherBillingFound)
            {
                CreateBillingFinancialQuery(contract, financialQuery, billingDtos, currentPeriod, startOfPeriod, endOfPeriod);
            }

            await ScanPeriodsForActiveContract(contract, endOfPeriod, financialDataRange, financialQuery, billingDtos, cancellationToken);
        }

        else if (financialDataRange != null)
        {
            (startOfPeriod, endOfPeriod) = GetPeriodBySpecificDate(contract, financialDataRange.MinDate);

            if (endOfPeriod > today) return;

            currentPeriod = endOfPeriod == today;

            CreateBillingFinancialQuery(contract, financialQuery, billingDtos, currentPeriod, startOfPeriod, endOfPeriod);

            await ScanPeriodsForActiveContract(contract, endOfPeriod, financialDataRange, financialQuery, billingDtos, cancellationToken);
        }

        if (!lastBillingDueDate.HasValue && financialDataRange == null)
        {
            (startOfPeriod, endOfPeriod) = GetPeriodBySpecificDate(contract, today);

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
            (startOfPeriod, endOfPeriod) = GetPeriodByLastBillingDueDate(contract, lastBillingDueDate.Value);
        }

        else
        {
            (startOfPeriod, endOfPeriod) = GetPeriodBySpecificDate(contract, financialDataRange.MinDate);
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

    private static (DateTime StartOfPeriod, DateTime EndOfPeriod) GetPeriodBySpecificDate(ContractGroup contract, DateTime specificDate)
    {
        int difference;
        DateTime startOfPeriod;
        DateTime endOfPeriod;

        var pc = new PersianCalendar();

        var year = pc.GetYear(specificDate);
        var month = pc.GetMonth(specificDate);
        var dayOfWeek = pc.GetDayOfWeek(specificDate);
        var dayOfMonth = pc.GetDayOfMonth(specificDate);

        var period = contract.BillingPeriod;

        switch (contract.BillingPeriodType)
        {
            case TimeInterval.Day:

                if (!contract.DailyBillingOriginDate.HasValue) throw new Exception();

                var originDate = contract.DailyBillingOriginDate.Value;

                if (specificDate.Date < originDate.Date)
                {
                    startOfPeriod = DateTime.MaxValue;
                    endOfPeriod = DateTime.MaxValue;
                    break;
                }

                var totalDays = (specificDate.Date - originDate.Date).Days;

                difference = period - (totalDays % period);

                startOfPeriod = pc.AddDays(specificDate, -difference);

                endOfPeriod = pc.AddDays(startOfPeriod, period);

                break;

            case TimeInterval.Week:

                if ((DayOfWeek)period >= dayOfWeek)
                {
                    difference = 7 - (period - (int)dayOfWeek);
                }
                else
                {
                    difference = (int)dayOfWeek - period;
                }

                startOfPeriod = pc.AddDays(specificDate, -difference);

                endOfPeriod = pc.AddWeeks(startOfPeriod, 1);

                break;

            case TimeInterval.Month:

                var regulatePeriod = DateHelper.RegulateBillingPeriod(pc, year, month, period);

                if (regulatePeriod >= dayOfMonth)
                {
                    startOfPeriod = pc.AddMonths(new DateTime(year, month, regulatePeriod, pc), -1);

                    startOfPeriod = DateHelper.RegulateDateOfPeriod(pc, startOfPeriod, period);
                }
                else
                {
                    startOfPeriod = pc.ToDateTime(year, month, period, 0, 0, 0, 0);
                }

                endOfPeriod = pc.AddMonths(startOfPeriod, 1);

                endOfPeriod = DateHelper.RegulateDateOfPeriod(pc, endOfPeriod, period);

                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        return new ValueTuple<DateTime, DateTime>(startOfPeriod, endOfPeriod);
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

                endOfPeriod = DateHelper.RegulateDateOfPeriod(pc, endOfPeriod, contract.BillingPeriod);

                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        return new ValueTuple<DateTime, DateTime>(startOfPeriod, endOfPeriod);
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