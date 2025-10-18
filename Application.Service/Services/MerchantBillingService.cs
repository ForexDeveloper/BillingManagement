using System;
using System.Linq;
using System.Threading;
using Domain.Core.Enums;
using System.Threading.Tasks;
using System.Collections.Generic;
using Application.Service.Helper;
using Domain.Core.Entities.Shared;
using Application.Service.Contracts;
using Domain.Core.UnitOfWorkContracts;
using Application.Service.Dtos.Shared;
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

        var billingsDtoSets = await CreateBillingDtos(groupContracts, jobCreatedDateTime, cancellationToken);

        await CreateMerchantBillings(billingsDtoSets, overdueBillings, negativeBillings, cancellationToken);
    }

    private async Task<Dictionary<TenantMerchantIdentifier, List<BillingDto>>> CreateBillingDtos(
        List<ContractGroup> contracts, DateTime jobCreatedDateTime, CancellationToken cancellationToken)
    {
        Dictionary<TenantMerchantIdentifier, List<BillingDto>> billingsDtoSets = [];

        foreach (var contract in contracts)
        {
            var billingDtos = new List<BillingDto>();

            var lastBillingDueDate = await merchantBillingRepository.GetLastBillingDueDate(contract.ContractIds, cancellationToken);

            var installmentRange = await merchantInstallmentRepository.GetInstallmentRange(contract.ContractIds, lastBillingDueDate, cancellationToken);

            var financialDocumentRange = await financialDocumentRepository.GetFinancialDocumentRange(contract.ContractIds, lastBillingDueDate, jobCreatedDateTime, cancellationToken);

            var financialDataRange = FinancialDataRange.Create(installmentRange, financialDocumentRange);

            switch (contract.Status)
            {
                case true:
                    await ScanPeriodsForActiveContract(contract, lastBillingDueDate, financialDataRange,
                        billingDtos, cancellationToken);
                    break;

                case false:
                    ScanPeriodsForDeactiveContract(contract, lastBillingDueDate, financialDataRange, billingDtos);
                    break;
            }

            if (billingDtos.Count == 0) continue;

            var tenantMerchantId = new TenantMerchantIdentifier(contract.TenantId, contract.MerchantId);

            if (!billingsDtoSets.TryAdd(tenantMerchantId, billingDtos))
            {
                billingsDtoSets.GetValueOrDefault(tenantMerchantId).AddRange(billingDtos);
            }
        }

        return billingsDtoSets;
    }

    private async Task CreateMerchantBillings(Dictionary<TenantMerchantIdentifier, List<BillingDto>> billingDtoGroups,
        List<NotSettledBilling> overdueBillings, List<NegativeSettledBilling> negativeBillings,
        CancellationToken cancellationToken)
    {
        foreach (var (_, billingDtos) in billingDtoGroups)
        {
            var oneDeactiveContractHasBilling = false;

            var billings = new List<MerchantBilling>();

            var replicateBillings = new List<MerchantBilling>();

            for (var i = 0; i < billingDtos.Count; i++)
            {
                decimal previousDebitAmount = 0;
                decimal previousCreditAmount = 0;
                decimal previousPenaltyAmount = 0;
                decimal sumOfTieredTransactions = 0;
                decimal purchaseTransactionsAmount = 0;
                decimal refundedTransactionsAmount = 0;
                decimal purchaseTransactionsCommission = 0;
                decimal refundedTransactionsCommission = 0;
                decimal purchaseTransactionsCalculatedCommission = 0;
                List<TieredCalculatedLevel> tieredCalculatedLevels = null;

                var billingDto = billingDtos[i];

                var contract = billingDto.ContractGroup;

                purchaseTransactionsAmount = await merchantInstallmentRepository.GetSumOfTransactionsInSpecificPeriod(contract.ContractIds,
                        billingDto.StartOfPeriod, billingDto.EndOfPeriod, cancellationToken);

                refundedTransactionsAmount = await financialDocumentRepository.GetSumOfRefundTransactionsInSpecificPeriod(contract.ContractIds,
                    billingDto.StartOfPeriod, billingDto.EndOfPeriod, cancellationToken);

                refundedTransactionsCommission = await financialDocumentRepository.GetSumOfRefundCommissionsInSpecificPeriod(contract.ContractIds,
                    billingDto.StartOfPeriod, billingDto.EndOfPeriod, cancellationToken);

                if (contract.CommissionCalculationType is CommissionCalculationType.FixedPercentage or CommissionCalculationType.FixedAmount)
                {
                    purchaseTransactionsCalculatedCommission = await merchantInstallmentRepository.GetSumOfCommissionsInSpecificPeriod(contract.ContractIds,
                        billingDto.StartOfPeriod, billingDto.EndOfPeriod, cancellationToken);
                }

                else if (contract.CommissionCalculationType == CommissionCalculationType.UniformTiered)
                {
                    sumOfTieredTransactions = await merchantInstallmentRepository.GetSumOfTieredTransactionsInSpecificPeriod(contract,
                        billingDto.StartOfPeriod, billingDto.EndOfPeriod, cancellationToken);

                    tieredCalculatedLevels = CalculateUniformedTieredLevels(contract.TieredCommissions, sumOfTieredTransactions);

                    purchaseTransactionsCalculatedCommission = tieredCalculatedLevels.Sum(p => p.Commission);
                }

                else if (contract.CommissionCalculationType == CommissionCalculationType.CumulativeTiered)
                {
                    sumOfTieredTransactions = await merchantInstallmentRepository.GetSumOfTieredTransactionsInSpecificPeriod(contract,
                        billingDto.StartOfPeriod, billingDto.EndOfPeriod, cancellationToken);

                    tieredCalculatedLevels = CalculateCumulativeTieredLevels(contract.TieredCommissions, sumOfTieredTransactions);

                    purchaseTransactionsCalculatedCommission = tieredCalculatedLevels.Sum(p => p.Commission);
                }

                MerchantBilling debtorBilling = null;
                MerchantBilling creditorBilling = null;

                purchaseTransactionsCommission = CalculateFinalCommission(contract, purchaseTransactionsCalculatedCommission);

                var debtorBillings = GetDebtorBillings(overdueBillings, contract.ContractIds);

                var creditorBillings = GetCreditorBillings(negativeBillings, contract.ContractIds);

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
                                replicateBilling.Transfer();
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
                    tieredCalculatedLevels,
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

    private async Task ScanPeriodsForActiveContract(ContractGroup contract, DateTime? lastBillingDueDate,
        FinancialDataRange financialDataRange, List<BillingDto> billingDtos,
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

            if (anotherBillingFound == false)
            {
                CreateBillingDto(contract, billingDtos, currentPeriod, startOfPeriod, endOfPeriod);
            }

            await ScanPeriodsForActiveContract(contract, endOfPeriod, financialDataRange, billingDtos, cancellationToken);
        }

        else if (financialDataRange != null)
        {
            (startOfPeriod, endOfPeriod) = ContractPeriodHelper.GetPeriodBySpecificDate(contract.BillingPeriod,
                contract.BillingPeriodType, contract.DailyBillingOriginDate, financialDataRange.MinDate);

            if (endOfPeriod > today) return;

            currentPeriod = endOfPeriod == today;

            CreateBillingDto(contract, billingDtos, currentPeriod, startOfPeriod, endOfPeriod);

            await ScanPeriodsForActiveContract(contract, endOfPeriod, financialDataRange, billingDtos, cancellationToken);
        }

        if (!lastBillingDueDate.HasValue && financialDataRange == null)
        {
            (startOfPeriod, endOfPeriod) = ContractPeriodHelper.GetPeriodBySpecificDate(contract.BillingPeriod,
                contract.BillingPeriodType, contract.DailyBillingOriginDate, today);

            currentPeriod = endOfPeriod == today;

            if (currentPeriod)
            {
                CreateBillingDto(contract, billingDtos, true, startOfPeriod, endOfPeriod);
            }
        }
    }

    private static void ScanPeriodsForDeactiveContract(ContractGroup contract, DateTime? lastBillingDueDate,
        FinancialDataRange financialDataRange, List<BillingDto> billingDtos)
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
            CreateBillingDto(contract, billingDtos, currentPeriod, startOfPeriod, endOfPeriod);
        }
        else
        {
            CreateBillingDto(contract, billingDtos, currentPeriod, startOfPeriod, endOfPeriod);

            ScanPeriodsForDeactiveContract(contract, endOfPeriod, financialDataRange, billingDtos);
        }
    }

    private static void CreateBillingDto(ContractGroup contract, List<BillingDto> billingDtos, bool currentPeriod,
        DateTime startOfPeriod, DateTime endOfPeriod)
    {
        var billingDto = new BillingDto
        {
            ContractGroup = contract,
            EndOfPeriod = endOfPeriod,
            StartOfPeriod = startOfPeriod,
            CurrentPeriod = currentPeriod
        };

        billingDtos.Add(billingDto);
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

        await unitOfWork.SaveChangesAsync(cancellationToken);

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

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return notSettledBillings;
    }

    private static decimal CalculateFinalCommission(ContractGroup contract, decimal purchaseTransactionsCalculatedCommission)
    {
        decimal finalCommission;

        if (purchaseTransactionsCalculatedCommission > contract.PeriodMaxCommissionAmount)
        {
            finalCommission = contract.PeriodMaxCommissionAmount.Value;
        }

        else if (purchaseTransactionsCalculatedCommission < contract.PeriodMinCommissionAmount)
        {
            finalCommission = contract.PeriodMinCommissionAmount.Value;
        }
        else
        {
            finalCommission = purchaseTransactionsCalculatedCommission;
        }

        return finalCommission;
    }

    private static List<TieredCalculatedLevel> CalculateUniformedTieredLevels(List<TieredCommission> tieredCommissions, decimal totalTransactionsAmount)
    {
        List<TieredCalculatedLevel> calculatedTieredLevels = [];

        if (tieredCommissions == null) return calculatedTieredLevels;

        var tieredCommission = tieredCommissions.FirstOrDefault(p =>
            p.FromAmount < totalTransactionsAmount && totalTransactionsAmount <= p.ToAmount);

        if (tieredCommission == null)
        {
            var minTieredCommission = tieredCommissions.MinBy(p => p.FromAmount);

            var maxTieredCommission = tieredCommissions.MaxBy(p => p.FromAmount);

            if (totalTransactionsAmount <= minTieredCommission.FromAmount)
            {
                tieredCommission = minTieredCommission;
            }

            else if (totalTransactionsAmount > maxTieredCommission.FromAmount)
            {
                tieredCommission = maxTieredCommission;
            }
        }

        if (tieredCommission == null) return calculatedTieredLevels;

        var commission = CalculateCommission(totalTransactionsAmount, tieredCommission);

        calculatedTieredLevels.Add(new TieredCalculatedLevel()
        {
            Number = 1,
            Commission = commission,
            TieredCommission = tieredCommission,
            TransactionsAmount = totalTransactionsAmount
        });

        return calculatedTieredLevels;
    }

    private static List<TieredCalculatedLevel> CalculateCumulativeTieredLevels(List<TieredCommission> tieredCommissions, decimal totalTransactionsAmount)
    {
        List<TieredCalculatedLevel> calculatedTieredLevels = [];

        if (tieredCommissions == null) return calculatedTieredLevels;

        var number = 1;

        var remainingAmount = totalTransactionsAmount;

        foreach (var tieredCommission in tieredCommissions.OrderBy(p => p.FromAmount))
        {
            decimal commission;

            decimal transactionsAmount;

            if (tieredCommission.ToAmount == null)
            {
                transactionsAmount = remainingAmount;

                commission = CalculateCommission(transactionsAmount, tieredCommission);

                calculatedTieredLevels.Add(new TieredCalculatedLevel()
                {
                    Number = number,
                    Commission = commission,
                    TieredCommission = tieredCommission,
                    TransactionsAmount = transactionsAmount
                });

                break;
            }

            var tieredTotalAmount = tieredCommission.ToAmount.Value - tieredCommission.FromAmount;

            if (remainingAmount >= tieredTotalAmount)
            {
                transactionsAmount = tieredTotalAmount;
            }
            else
            {
                transactionsAmount = remainingAmount;
            }

            commission = CalculateCommission(transactionsAmount, tieredCommission);

            calculatedTieredLevels.Add(new TieredCalculatedLevel()
            {
                Number = number,
                Commission = commission,
                TieredCommission = tieredCommission,
                TransactionsAmount = transactionsAmount
            });

            remainingAmount -= tieredTotalAmount;

            if (remainingAmount <= 0) break;

            number++;
        }

        return calculatedTieredLevels;
    }

    private static decimal CalculateCommission(decimal targetAmount, TieredCommission tieredCommission)
    {
        var commission = targetAmount * (tieredCommission.Percentage / 100);

        commission = RoundHelper.RoundAmount(commission);

        if (commission > tieredCommission.MaxAmount)
        {
            commission = tieredCommission.MaxAmount.Value;
        }

        if (commission < tieredCommission.MinAmount)
        {
            commission = tieredCommission.MinAmount.Value;
        }

        return commission;
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