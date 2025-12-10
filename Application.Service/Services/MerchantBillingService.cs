using System;
using System.Linq;
using System.Threading;
using Domain.Core.Enums;
using System.Threading.Tasks;
using System.Collections.Generic;
using Application.Service.Helper;
using Domain.Core.Entities.Shared;
using Microsoft.Extensions.Logging;
using Application.Service.Contracts;
using Domain.Core.UnitOfWorkContracts;
using Shared.Logging.Abstraction.Models;
using Shared.Logging.Abstraction.Extensions;
using Domain.Core.Entities.BillingAggregate.Dtos;
using Domain.Core.Entities.MerchantBillingAggregate;
using Domain.Core.Entities.InstallmentAggregate.Dtos;
using Domain.Core.Entities.MerchantInstallmentAggregate;
using Domain.Core.Entities.TenantMerchantContractAggregate;
using Domain.Core.Entities.TenantMerchantContractAggregate.Dtos;

namespace Application.Service.Services;

public sealed class MerchantBillingService(
    ILogger<MerchantBillingService> logger,
    IApplicationDbContextUnitOfWork unitOfWork,
    IMerchantBillingRepository merchantBillingRepository,
    IMerchantInstallmentRepository merchantInstallmentRepository,
    ITenantMerchantContractRepository tenantMerchantContractRepository) : IMerchantBillingService
{
    public async Task IssueOrOverdueBillings(CancellationToken cancellationToken)
    {
        var negativeBillings = await merchantBillingRepository.GetNegativeSettledBillings(cancellationToken);

        var notSettledBillings = await merchantBillingRepository.GetOverdueOrNotSettledBillings(cancellationToken);

        var overdueBillings = await OverdueExpiredBillings(notSettledBillings, cancellationToken);

        var groupContracts = await tenantMerchantContractRepository.GetAllGroupContractAsync(cancellationToken);

        var billingsDtoSets = await CreateBillingDtos(groupContracts, cancellationToken);

        await CreateMerchantBillings(billingsDtoSets, overdueBillings, negativeBillings, cancellationToken);
    }

    private async Task<Dictionary<BillingIdentifier, List<BillingDto>>> CreateBillingDtos(
        List<ContractGroup> contracts, CancellationToken cancellationToken)
    {
        Dictionary<BillingIdentifier, List<BillingDto>> billingsDtoSets = [];

        foreach (var contract in contracts)
        {
            try
            {
                var billingDtos = new List<BillingDto>();

                var lastBillingDueDate = await merchantBillingRepository.GetLastBillingDueDate(contract.ContractIds, cancellationToken);

                var installmentRange = await merchantInstallmentRepository.GetInstallmentRange(contract.ContractIds, lastBillingDueDate, cancellationToken);

                switch (contract.Status)
                {
                    case true:
                        await ScanPeriodsForActiveContract(contract, lastBillingDueDate, installmentRange,
                            billingDtos, cancellationToken);
                        break;

                    case false:
                        ScanPeriodsForDeactiveContract(contract, lastBillingDueDate, installmentRange, billingDtos);
                        break;
                }

                if (billingDtos.Count == 0) continue;

                var tenantToMerchantBillings = billingDtos.Where(p => p.Type == BillingType.TenantToMerchant).ToList();

                var billingIdentifier = new BillingIdentifier(contract.TenantId, contract.MerchantId, BillingType.TenantToMerchant);

                if (!billingsDtoSets.TryAdd(billingIdentifier, tenantToMerchantBillings))
                {
                    billingsDtoSets.GetValueOrDefault(billingIdentifier).AddRange(tenantToMerchantBillings);
                }

                var merchantToTenantBillings = billingDtos.Where(p => p.Type == BillingType.MerchantToTenant).ToList();

                if (merchantToTenantBillings.Count == 0) continue;

                billingIdentifier = new BillingIdentifier(contract.TenantId, contract.MerchantId, BillingType.MerchantToTenant);

                if (!billingsDtoSets.TryAdd(billingIdentifier, merchantToTenantBillings))
                {
                    billingsDtoSets.GetValueOrDefault(billingIdentifier).AddRange(merchantToTenantBillings);
                }
            }
            catch (Exception exception)
            {
                logger.LogCritical(new LogStruct()
                {
                    Results = "",
                    Exception = exception,
                    InputParams = contract,
                    Message = "MerchantBillingService creating billing dtos failed",
                    ServiceName = $"{nameof(MerchantBillingService)}_{nameof(CreateBillingDtos)}"
                });
            }
        }

        return billingsDtoSets;
    }

    private async Task CreateMerchantBillings(Dictionary<BillingIdentifier, List<BillingDto>> billingDtoGroups,
        List<NotSettledBilling> overdueBillings, List<NegativeSettledBilling> negativeBillings,
        CancellationToken cancellationToken)
    {
        foreach (var (billingIdentifier, billingDtos) in billingDtoGroups)
        {
            try
            {
                var oneDeactiveContractHasBilling = false;

                var billings = new List<MerchantBilling>();

                var replicateBillings = new List<MerchantBilling>();

                foreach (var billingDto in billingDtos)
                {
                    var contract = billingDto.ContractGroup;

                    decimal previousDebitAmount = 0;
                    decimal previousCreditAmount = 0;
                    decimal previousPenaltyAmount = 0;

                    MerchantBilling debtorBilling = null;
                    MerchantBilling creditorBilling = null;

                    var replicateBilling = replicateBillings.LastOrDefault();

                    if (replicateBilling != null)
                    {
                        var previousIndex = billingDtos.IndexOf(billingDto) - 1;

                        var previousContract = billingDtos[previousIndex].ContractGroup;

                        var contractIdentifier = contract.CreateIdentifier();

                        var previousContractIdentifier = previousContract.CreateIdentifier();

                        if (contractIdentifier == previousContractIdentifier)
                        {
                            var payableAmount = replicateBilling.GetPayableAmount();

                            if (billingDto.Type == BillingType.TenantToMerchant)
                            {
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
                                        replicateBilling.Transfer();
                                        creditorBilling = replicateBilling;
                                        previousCreditAmount = Math.Abs(payableAmount);
                                        break;
                                }
                            }
                            else if (billingDto.Type == BillingType.MerchantToTenant)
                            {
                                replicateBilling.Overdue();
                                replicateBilling.Transfer();
                                debtorBilling = replicateBilling;
                                previousDebitAmount = payableAmount;
                            }
                        }
                        else
                        {
                            replicateBillings.Clear();
                        }
                    }

                    if (replicateBillings.Count == 0)
                    {
                        debtorBilling = TransferDebtorBillings(overdueBillings,
                            billingDto,
                            contract.ContractIds,
                            out previousDebitAmount);

                        creditorBilling = TransferCreditorBillings(negativeBillings,
                            billingDto,
                            contract.ContractIds,
                            out previousCreditAmount);
                    }

                    Commission purchaseCommission = new();
                    Transactions refundedTransactions = new();

                    if (contract.IsCommissionExchanged)
                    {
                        refundedTransactions = await CalculateRefundedTransactions(billingDto, cancellationToken);

                        purchaseCommission = await CalculateTransactionsCommission(billingDto, cancellationToken);
                    }
                    else
                    {
                        if (billingDto.Type == BillingType.TenantToMerchant)
                        {
                            refundedTransactions = await CalculateRefundedTransactions(billingDto, cancellationToken);

                        }
                        else if (billingDto.Type == BillingType.MerchantToTenant)
                        {
                            purchaseCommission = await CalculateTransactionsCommission(billingDto, cancellationToken);
                        }
                    }

                    var purchaseTransactionsAmount = await merchantInstallmentRepository.GetSumOfPurchaseTransactionsInSpecificPeriod(
                            contract.ContractIds, billingDto.StartOfPeriod, billingDto.EndOfPeriod, cancellationToken);

                    var refundedTransactionsAmount = refundedTransactions.RefundedTransactionsAmount;
                    var refundedTransactionsCommission = refundedTransactions.RefundedTransactionsCommission;

                    var tieredCalculatedLevels = purchaseCommission.TieredCalculatedLevels;
                    var tieredTransactionsAmount = purchaseCommission.TieredTransactionsAmount;
                    var purchaseTransactionsCommission = purchaseCommission.PurchaseTransactionsCommission;
                    var purchaseTransactionsCalculatedCommission = purchaseCommission.PurchaseTransactionsCalculatedCommission;

                    var billing = new MerchantBilling(contract.TenantId,
                        billingDto.FromBusinessIdentityId,
                        billingDto.ToBusinessIdentityId,
                        billingDto.Type,
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
                        tieredTransactionsAmount,
                        tieredCalculatedLevels,
                        debtorBilling,
                        creditorBilling);

                    replicateBillings.Add(billing);

                    if (billing.IsAbsoluteZero())
                    {
                        if (!contract.Status) continue;

                        //if (contract.Status && !billingDto.CurrentPeriod) continue;

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

            catch (Exception exception)
            {
                logger.LogCritical(new LogStruct()
                {
                    Results = "",
                    Exception = exception,
                    InputParams = billingIdentifier,
                    Message = "MerchantBillingService billings issue failed",
                    ServiceName = $"{nameof(MerchantBillingService)}_{nameof(CreateMerchantBillings)}"
                });
            }
            finally
            {
                logger.LogTrace(new LogStruct()
                {
                    Results = "",
                    Exception = null,
                    InputParams = billingIdentifier,
                    Message = "MerchantBillingService billings issue completed",
                    ServiceName = $"{nameof(MerchantBillingService)}_{nameof(CreateMerchantBillings)}"
                });
            }
        }
    }

    private async Task ScanPeriodsForActiveContract(ContractGroup contract, DateTime? lastBillingDueDate,
        InstallmentRange installmentRange, List<BillingDto> billingDtos,
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

            if (endOfPeriod > installmentRange?.MaxDueDate)
            {
                var hasIntersection = await merchantBillingRepository.HasIntersectionWithAnotherBillingPeriod(contract.TenantId,
                    contract.MerchantId, endOfPeriod, cancellationToken);

                if (hasIntersection == false)
                {
                    CreateBillingDto(contract, billingDtos, currentPeriod, startOfPeriod, endOfPeriod);
                }
            }
            else
            {
                CreateBillingDto(contract, billingDtos, currentPeriod, startOfPeriod, endOfPeriod);
            }

            await ScanPeriodsForActiveContract(contract, endOfPeriod, installmentRange, billingDtos, cancellationToken);
        }

        else if (installmentRange != null)
        {
            (startOfPeriod, endOfPeriod) = ContractPeriodHelper.GetPeriodBySpecificDate(contract.BillingPeriod,
                contract.BillingPeriodType, contract.DailyBillingOriginDate, installmentRange.MinDueDate);

            if (endOfPeriod > today) return;

            currentPeriod = endOfPeriod == today;

            CreateBillingDto(contract, billingDtos, currentPeriod, startOfPeriod, endOfPeriod);

            await ScanPeriodsForActiveContract(contract, endOfPeriod, installmentRange, billingDtos, cancellationToken);
        }

        if (!lastBillingDueDate.HasValue && installmentRange == null)
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
        InstallmentRange installmentRange, List<BillingDto> billingDtos)
    {
        DateTime endOfPeriod;
        DateTime startOfPeriod;

        var today = DateTime.Today;

        if (installmentRange == null) return;

        if (lastBillingDueDate.HasValue)
        {
            (startOfPeriod, endOfPeriod) = ContractPeriodHelper.GetPeriodByLastBillingDueDate(contract.BillingPeriod,
                contract.BillingPeriodType, contract.DailyBillingOriginDate, lastBillingDueDate.Value);
        }
        else
        {
            (startOfPeriod, endOfPeriod) = ContractPeriodHelper.GetPeriodBySpecificDate(contract.BillingPeriod,
                contract.BillingPeriodType, contract.DailyBillingOriginDate, installmentRange.MinDueDate);
        }

        if (endOfPeriod > today) return;

        var currentPeriod = endOfPeriod == today;

        if (endOfPeriod > installmentRange.MaxDueDate)
        {
            CreateBillingDto(contract, billingDtos, currentPeriod, startOfPeriod, endOfPeriod);
        }
        else
        {
            CreateBillingDto(contract, billingDtos, currentPeriod, startOfPeriod, endOfPeriod);

            ScanPeriodsForDeactiveContract(contract, endOfPeriod, installmentRange, billingDtos);
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
            CurrentPeriod = currentPeriod,
            Type = BillingType.TenantToMerchant,
            FromBusinessIdentityId = contract.TenantId,
            ToBusinessIdentityId = contract.MerchantId
        };

        billingDtos.Add(billingDto);

        if (!contract.IsCommissionExchanged)
        {
            billingDto = new BillingDto
            {
                ContractGroup = contract,
                EndOfPeriod = endOfPeriod,
                StartOfPeriod = startOfPeriod,
                CurrentPeriod = currentPeriod,
                Type = BillingType.MerchantToTenant,
                FromBusinessIdentityId = contract.MerchantId,
                ToBusinessIdentityId = contract.TenantId
            };

            billingDtos.Add(billingDto);
        }
    }

    private async Task<List<NotSettledBilling>> OverdueExpiredBillings(List<NotSettledBilling> notSettledBillings, CancellationToken cancellationToken)
    {
        try
        {
            for (var i = notSettledBillings.Count - 1; i >= 0; i--)
            {
                var billing = notSettledBillings[i].Billing;

                if (billing.Status is BillingStatus.Issued or BillingStatus.PartiallyPaid)
                {
                    var payableAmount = billing.GetPayableAmount();

                    if (payableAmount > 0)
                    {
                        billing.Overdue();
                    }
                    else
                    {
                        notSettledBillings.RemoveAt(i);
                    }
                }
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return notSettledBillings;
        }
        catch (Exception exception)
        {
            logger.LogCritical(new LogStruct()
            {
                Results = "",
                InputParams = "",
                Exception = exception,
                Message = "MerchantBillingService billings overdue failed",
                ServiceName = $"{nameof(MerchantBillingService)}_{nameof(OverdueExpiredBillings)}"
            });

            return [];
        }
        finally
        {
            logger.LogTrace(new LogStruct()
            {
                Results = "",
                InputParams = "",
                Exception = null,
                Message = "MerchantBillingService billings overdue completed",
                ServiceName = $"{nameof(MerchantBillingService)}_{nameof(OverdueExpiredBillings)}"
            });
        }
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
        List<TieredCalculatedLevel> tieredCalculatedLevels = [];

        if (tieredCommissions == null) return tieredCalculatedLevels;

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

        if (tieredCommission == null) return tieredCalculatedLevels;

        var commission = CalculateCommission(totalTransactionsAmount, tieredCommission);

        tieredCalculatedLevels.Add(new TieredCalculatedLevel()
        {
            Commission = commission,
            TieredCommission = tieredCommission,
            TransactionsAmount = totalTransactionsAmount,
            Number = tieredCommissions.IndexOf(tieredCommission) + 1
        });

        return tieredCalculatedLevels;
    }

    private static List<TieredCalculatedLevel> CalculateCumulativeTieredLevels(List<TieredCommission> tieredCommissions, decimal totalTransactionsAmount)
    {
        List<TieredCalculatedLevel> tieredCalculatedLevels = [];

        if (tieredCommissions == null) return tieredCalculatedLevels;

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

                tieredCalculatedLevels.Add(new TieredCalculatedLevel()
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

            tieredCalculatedLevels.Add(new TieredCalculatedLevel()
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

        return tieredCalculatedLevels;
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

    private static MerchantBilling TransferDebtorBillings(List<NotSettledBilling> overdueBillings, BillingDto billingDto, List<int> contractIds, out decimal previousDebitAmount)
    {
        previousDebitAmount = 0;
        MerchantBilling debtorBilling = null;
        List<MerchantBilling> debtorBillings = [];

        foreach (var overdueBilling in overdueBillings.Where(p =>
                     p.Billing.Type == billingDto.Type && p.Billing.DueDate < billingDto.EndOfPeriod))
        {
            var billing = overdueBilling.Billing;

            if (contractIds.Any(billing.ContractIds.Contains))
            {
                billing.Transfer();

                previousDebitAmount += billing.GetPayableAmount();

                debtorBilling = billing;

                debtorBillings.Add(billing);
            }

            else if (contractIds.Contains(overdueBilling.ActiveContractId))
            {
                billing.Transfer();

                previousDebitAmount += billing.GetPayableAmount();

                debtorBilling ??= billing;

                debtorBillings.Add(billing);
            }
        }

        overdueBillings.RemoveAll(p => debtorBillings.Contains(p.Billing));

        return debtorBilling;
    }

    private static MerchantBilling TransferCreditorBillings(List<NegativeSettledBilling> negativeBillings, BillingDto billingDto, List<int> contractIds, out decimal previousCreditAmount)
    {
        previousCreditAmount = 0;
        MerchantBilling creditorBilling = null;
        List<MerchantBilling> creditorBillings = [];

        foreach (var negativeBilling in negativeBillings.Where(p =>
                     p.Billing.Type == billingDto.Type && p.Billing.DueDate < billingDto.EndOfPeriod))
        {
            var billing = negativeBilling.Billing;

            if (contractIds.Any(billing.ContractIds.Contains))
            {
                billing.Transfer();

                previousCreditAmount += Math.Abs(billing.GetPayableAmount());

                creditorBilling = billing;

                creditorBillings.Add(billing);
            }

            else if (contractIds.Contains(negativeBilling.ActiveContractId))
            {
                billing.Transfer();

                previousCreditAmount += Math.Abs(billing.GetPayableAmount());

                creditorBilling ??= billing;

                creditorBillings.Add(billing);
            }
        }

        negativeBillings.RemoveAll(p => creditorBillings.Contains(p.Billing));

        return creditorBilling;
    }

    private async Task<Transactions> CalculateRefundedTransactions(BillingDto billingDto, CancellationToken cancellationToken)
    {
        var contract = billingDto.ContractGroup;

        var refundedTransactionsAmount = await merchantInstallmentRepository.GetSumOfRefundTransactionsInSpecificPeriod(
            contract.ContractIds, billingDto.StartOfPeriod, billingDto.EndOfPeriod, cancellationToken);

        var refundedTransactionsCommission = await merchantInstallmentRepository.GetSumOfRefundCommissionsInSpecificPeriod(
            contract.ContractIds, billingDto.StartOfPeriod, billingDto.EndOfPeriod, cancellationToken);

        return new Transactions()
        {
            RefundedTransactionsAmount = refundedTransactionsAmount,
            RefundedTransactionsCommission = refundedTransactionsCommission
        };
    }

    private async Task<Commission> CalculateTransactionsCommission(BillingDto billingDto, CancellationToken cancellationToken)
    {
        decimal sumOfTieredTransactions = 0;
        decimal purchaseTransactionsCalculatedCommission = 0;
        List<TieredCalculatedLevel> tieredCalculatedLevels = null;

        var contract = billingDto.ContractGroup;

        if (contract.CommissionCalculationType is CommissionCalculationType.FixedAmount or CommissionCalculationType.FixedPercentage)
        {
            purchaseTransactionsCalculatedCommission = await merchantInstallmentRepository.GetSumOfPurchaseCommissionsInSpecificPeriod(contract.ContractIds,
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

        var purchaseTransactionsCommission = CalculateFinalCommission(contract, purchaseTransactionsCalculatedCommission);

        return new Commission()
        {
            TieredCalculatedLevels = tieredCalculatedLevels,
            TieredTransactionsAmount = sumOfTieredTransactions,
            PurchaseTransactionsCommission = purchaseTransactionsCommission,
            PurchaseTransactionsCalculatedCommission = purchaseTransactionsCalculatedCommission
        };
    }

    private static bool ShouldSkipBilling(decimal purchaseTransactionsAmount, bool contractStatus, bool isCurrentPeriod, ref bool oneDeactiveContractHasBilling)
    {
        if (purchaseTransactionsAmount == 0)
        {
            if (!contractStatus) return true;

            //if (!isCurrentPeriod) return true;

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

    private sealed record Commission
    {
        public decimal TieredTransactionsAmount { get; init; }

        public decimal PurchaseTransactionsCommission { get; init; }

        public decimal PurchaseTransactionsCalculatedCommission { get; init; }

        public List<TieredCalculatedLevel> TieredCalculatedLevels { get; init; }
    }

    private sealed record Transactions
    {
        public decimal RefundedTransactionsAmount { get; init; }

        public decimal RefundedTransactionsCommission { get; init; }
    }
}