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
                decimal currentPeriodFinalCommission = 0;
                decimal refundedTransactionsCommission = 0;
                decimal currentPeriodCalculatedCommission = 0;
                decimal currentPeriodPurchaseTransactions = 0;
                decimal previousPeriodRefundedTransactions = 0;

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
                    currentPeriodPurchaseTransactions += installment.Amount;
                    currentPeriodCalculatedCommission += installment.Commission;
                }

                foreach (var financialDocumentDto in financialDocuments)
                {
                    switch (financialDocumentDto.Type)
                    {
                        case FinancialDocumentType.Purchase:
                            purchaseFinancialDocuments.Add(financialDocumentDto);
                            break;

                        case FinancialDocumentType.Refund:
                            previousPeriodRefundedTransactions += financialDocumentDto.Amount;
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
                    currentPeriodCalculatedCommission = CalculateUniformedTieredCommission(contract, currentPeriodPurchaseTransactions);
                }

                currentPeriodFinalCommission = CalculateFinalCommission(contract, currentPeriodCalculatedCommission);

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

                        debtorBilling = debtorBillings.FirstOrDefault();

                        creditorBilling = creditorBillings.FirstOrDefault();

                        previousDebitAmount = debtorBillings.Sum(p => p.GetPayableAmount());

                        previousCreditAmount = creditorBillings.Sum(p => p.GetPayableAmount());
                    }
                }
                else
                {
                    debtorBilling = debtorBillings.FirstOrDefault();

                    creditorBilling = creditorBillings.FirstOrDefault();

                    previousDebitAmount = debtorBillings.Sum(p => p.GetPayableAmount());

                    previousCreditAmount = creditorBillings.Sum(p => p.GetPayableAmount());
                }

                var billing = new MerchantBilling(contract.TenantId, contract.TenantId, contract.MerchantId,
                    BillingType.TenantToMerchant, contract.BillingPeriodType, previousDebitAmount, previousCreditAmount, 0,
                    billingDto.StartOfPeriod, billingDto.EndOfPeriod, 0, billingDto.ContractIds,
                    currentPeriodCalculatedCommission, currentPeriodFinalCommission, refundedTransactionsCommission,
                    previousPeriodRefundedTransactions, currentPeriodPurchaseTransactions, installments,
                    debtorBilling, creditorBilling);

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

        foreach (var contract in allContracts)
        {
            var billingDtos = new List<BillingDto>();

            var lastBillingEndDate = await merchantBillingRepository.GetLastBillingDueDate(contract.ContractIds, cancellationToken);

            switch (contract.BillingPeriodType, contract.Status)
            {
                case (TimeInterval.Day, true):
                    ScanDailyBillingsForActiveContract(contract, lastBillingEndDate, financialQuery, billingDtos);
                    break;

                case (TimeInterval.Day, false):
                    await ScanDailyBillingsForDeactiveContract(contract, lastBillingEndDate, financialQuery, billingDtos, cancellationToken);
                    break;

                case (TimeInterval.Week, true):
                    ScanWeeklyBillingsForActiveContract(contract, lastBillingEndDate, financialQuery, billingDtos);
                    break;

                case (TimeInterval.Week, false):
                    await ScanWeeklyBillingsForDeactiveContract(contract, lastBillingEndDate, financialQuery, billingDtos, cancellationToken);
                    break;

                case (TimeInterval.Month, true):
                    ScanMonthlyBillingsForActiveContract(contract, lastBillingEndDate, financialQuery, billingDtos);
                    break;

                case (TimeInterval.Month, false):
                    await ScanMonthlyBillingsForDeactiveContract(contract, lastBillingEndDate, financialQuery, billingDtos, cancellationToken);
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

    private void ScanDailyBillingsForActiveContract(ContractGroup contract, DateTime? lastBillingDueDate,
    FinancialQuery financialQuery, List<BillingDto> billingDtos)
    {
        DateTime endOfPeriod;
        DateTime startOfPeriod;

        var pc = new PersianCalendar();

        var today = DateTime.Today;
        var period = contract.BillingPeriod;

        if (lastBillingDueDate.HasValue)
        {
            startOfPeriod = lastBillingDueDate.Value;

            endOfPeriod = pc.AddDays(lastBillingDueDate.Value, period);

            if (endOfPeriod < today)
            {
                var billingDto = new BillingDto
                {
                    CurrentPeriod = false,
                    ContractGroup = contract,
                    EndOfPeriod = endOfPeriod,
                    StartOfPeriod = startOfPeriod,
                    ContractIds = contract.ContractIds
                };

                billingDtos.Add(billingDto);

                var installmentQuery = merchantInstallmentRepository.CreateJobInstallmentQuery(startOfPeriod, endOfPeriod, contract.ContractIds);

                var financialDocumentQuery = financialDocumentRepository.CreateJobFinancialDocumentQuery(startOfPeriod, endOfPeriod, contract.ContractIds);

                financialQuery.BuildQueries(installmentQuery, financialDocumentQuery);

                ScanDailyBillingsForActiveContract(contract, endOfPeriod, financialQuery, billingDtos);
            }

            else if (endOfPeriod == today)
            {
                var billingDto = new BillingDto
                {
                    CurrentPeriod = true,
                    ContractGroup = contract,
                    EndOfPeriod = endOfPeriod,
                    StartOfPeriod = startOfPeriod,
                    ContractIds = contract.ContractIds
                };

                var installmentQuery = merchantInstallmentRepository.CreateJobInstallmentQuery(startOfPeriod, endOfPeriod, contract.ContractIds);

                var financialDocumentQuery = financialDocumentRepository.CreateJobFinancialDocumentQuery(startOfPeriod, endOfPeriod, contract.ContractIds);

                financialQuery.BuildQueries(installmentQuery, financialDocumentQuery);

                billingDtos.Add(billingDto);
            }
        }
        else
        {
            if (!contract.BillingDailyOriginDate.HasValue) return;

            var originDate = contract.BillingDailyOriginDate.Value;

            if (today.Date < originDate.Date) return;

            var totalDays = (today.Date - originDate.Date).Days;

            var currentPeriod = totalDays % period == 0;

            if (currentPeriod)
            {
                endOfPeriod = today;

                startOfPeriod = pc.AddDays(endOfPeriod, -period);

                var billingDto = new BillingDto
                {
                    CurrentPeriod = true,
                    ContractGroup = contract,
                    EndOfPeriod = endOfPeriod,
                    StartOfPeriod = startOfPeriod,
                    ContractIds = contract.ContractIds
                };

                var installmentQuery = merchantInstallmentRepository.CreateJobInstallmentQuery(startOfPeriod, endOfPeriod, contract.ContractIds);

                var financialDocumentQuery = financialDocumentRepository.CreateJobFinancialDocumentQuery(startOfPeriod, endOfPeriod, contract.ContractIds);

                financialQuery.BuildQueries(installmentQuery, financialDocumentQuery);

                billingDtos.Add(billingDto);
            }
        }
    }

    private async Task ScanDailyBillingsForDeactiveContract(ContractGroup contract, DateTime? lastBillingDueDate,
        FinancialQuery financialQuery, List<BillingDto> billingDtos, CancellationToken cancellationToken)
    {
        DateTime endOfPeriod;
        DateTime startOfPeriod;

        var pc = new PersianCalendar();

        var today = DateTime.Today;
        var period = contract.BillingPeriod;
        var endorsementDate = contract.EndorsementDate;

        var lastInstallmentDueDate = await merchantInstallmentRepository.GetLastInstallmentDueDate(contract.ContractIds, cancellationToken);

        if (!lastBillingDueDate.HasValue)
        {
            return;
        }

        if ((endorsementDate <= lastBillingDueDate) ||
            (lastBillingDueDate < endorsementDate && endorsementDate <= lastInstallmentDueDate))
        {
            startOfPeriod = lastBillingDueDate.Value;

            endOfPeriod = pc.AddDays(lastBillingDueDate.Value, period);

            if (endOfPeriod <= lastInstallmentDueDate)
            {
                var billingDto = new BillingDto
                {
                    CurrentPeriod = false,
                    ContractGroup = contract,
                    EndOfPeriod = endOfPeriod,
                    StartOfPeriod = startOfPeriod,
                    ContractIds = contract.ContractIds
                };

                billingDtos.Add(billingDto);

                var installmentQuery = merchantInstallmentRepository.CreateJobInstallmentQuery(startOfPeriod, endOfPeriod, contract.ContractIds);

                var financialDocumentQuery = financialDocumentRepository.CreateJobFinancialDocumentQuery(startOfPeriod, endOfPeriod, contract.ContractIds);

                financialQuery.BuildQueries(installmentQuery, financialDocumentQuery);

                await ScanDailyBillingsForDeactiveContract(contract, endOfPeriod, financialQuery, billingDtos, cancellationToken);
            }
            else
            {
                var currentPeriod = endOfPeriod == today;

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
        }

        else if (endorsementDate > lastInstallmentDueDate)
        {
            startOfPeriod = lastBillingDueDate.Value;

            endOfPeriod = pc.AddDays(lastBillingDueDate.Value, period);

            if (endOfPeriod <= endorsementDate)
            {
                var billingDto = new BillingDto
                {
                    CurrentPeriod = false,
                    ContractGroup = contract,
                    EndOfPeriod = endOfPeriod,
                    StartOfPeriod = startOfPeriod,
                    ContractIds = contract.ContractIds
                };

                billingDtos.Add(billingDto);

                var installmentQuery = merchantInstallmentRepository.CreateJobInstallmentQuery(startOfPeriod, endOfPeriod, contract.ContractIds);

                var financialDocumentQuery = financialDocumentRepository.CreateJobFinancialDocumentQuery(startOfPeriod, endOfPeriod, contract.ContractIds);

                financialQuery.BuildQueries(installmentQuery, financialDocumentQuery);

                await ScanDailyBillingsForDeactiveContract(contract, endOfPeriod, financialQuery, billingDtos, cancellationToken);
            }
        }
    }

    private void ScanWeeklyBillingsForActiveContract(ContractGroup contract, DateTime? lastBillingEndDate,
        FinancialQuery financialQuery, List<BillingDto> billingDtos)
    {
        DateTime endOfPeriod;
        DateTime startOfPeriod;

        var today = DateTime.Today;

        var pc = new PersianCalendar();

        var period = contract.BillingPeriod;

        if (lastBillingEndDate.HasValue)
        {
            startOfPeriod = lastBillingEndDate.Value;

            endOfPeriod = pc.AddWeeks(lastBillingEndDate.Value, 1);

            if (endOfPeriod < today)
            {
                var billingDto = new BillingDto
                {
                    CurrentPeriod = false,
                    ContractGroup = contract,
                    EndOfPeriod = endOfPeriod,
                    StartOfPeriod = startOfPeriod,
                    ContractIds = contract.ContractIds
                };

                billingDtos.Add(billingDto);

                var installmentQuery = merchantInstallmentRepository.CreateJobInstallmentQuery(startOfPeriod, endOfPeriod, contract.ContractIds);

                var financialDocumentQuery = financialDocumentRepository.CreateJobFinancialDocumentQuery(startOfPeriod, endOfPeriod, contract.ContractIds);

                financialQuery.BuildQueries(installmentQuery, financialDocumentQuery);

                ScanWeeklyBillingsForActiveContract(contract, endOfPeriod, financialQuery, billingDtos);
            }

            else if (endOfPeriod == today)
            {
                var billingDto = new BillingDto
                {
                    CurrentPeriod = true,
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
        }
        else
        {
            var dayOfWeek = pc.GetDayOfWeek(today);

            var currentPeriod = (DayOfWeek)period == dayOfWeek;

            if (currentPeriod)
            {
                endOfPeriod = today;

                startOfPeriod = pc.AddWeeks(endOfPeriod, -1);

                var billingDto = new BillingDto
                {
                    CurrentPeriod = true,
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
        }
    }

    private async Task ScanWeeklyBillingsForDeactiveContract(ContractGroup contract, DateTime? lastBillingDueDate,
        FinancialQuery financialQuery, List<BillingDto> billingDtos, CancellationToken cancellationToken)
    {
        DateTime endOfPeriod;
        DateTime startOfPeriod;

        var pc = new PersianCalendar();

        var today = DateTime.Today;
        var endorsementDate = contract.EndorsementDate;

        var lastInstallmentDueDate = await merchantInstallmentRepository.GetLastInstallmentDueDate(contract.ContractIds, cancellationToken);

        if (!lastBillingDueDate.HasValue)
        {
            return;
        }

        if ((endorsementDate <= lastBillingDueDate) ||
            (lastBillingDueDate < endorsementDate && endorsementDate <= lastInstallmentDueDate))
        {
            startOfPeriod = lastBillingDueDate.Value;

            endOfPeriod = pc.AddWeeks(lastBillingDueDate.Value, 1);

            if (endOfPeriod <= lastInstallmentDueDate)
            {
                var billingDto = new BillingDto
                {
                    CurrentPeriod = false,
                    ContractGroup = contract,
                    EndOfPeriod = endOfPeriod,
                    StartOfPeriod = startOfPeriod,
                    ContractIds = contract.ContractIds
                };

                billingDtos.Add(billingDto);

                var installmentQuery = merchantInstallmentRepository.CreateJobInstallmentQuery(startOfPeriod, endOfPeriod, contract.ContractIds);

                var financialDocumentQuery = financialDocumentRepository.CreateJobFinancialDocumentQuery(startOfPeriod, endOfPeriod, contract.ContractIds);

                financialQuery.BuildQueries(installmentQuery, financialDocumentQuery);

                await ScanWeeklyBillingsForDeactiveContract(contract, endOfPeriod, financialQuery, billingDtos, cancellationToken);
            }
            else
            {
                var currentPeriod = endOfPeriod == today;

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
        }

        else if (endorsementDate > lastInstallmentDueDate)
        {
            startOfPeriod = lastBillingDueDate.Value;

            endOfPeriod = pc.AddWeeks(lastBillingDueDate.Value, 1);

            if (endOfPeriod <= endorsementDate)
            {
                var billingDto = new BillingDto
                {
                    CurrentPeriod = false,
                    ContractGroup = contract,
                    EndOfPeriod = endOfPeriod,
                    StartOfPeriod = startOfPeriod,
                    ContractIds = contract.ContractIds
                };

                billingDtos.Add(billingDto);

                var installmentQuery = merchantInstallmentRepository.CreateJobInstallmentQuery(startOfPeriod, endOfPeriod, contract.ContractIds);

                var financialDocumentQuery = financialDocumentRepository.CreateJobFinancialDocumentQuery(startOfPeriod, endOfPeriod, contract.ContractIds);

                financialQuery.BuildQueries(installmentQuery, financialDocumentQuery);

                await ScanWeeklyBillingsForDeactiveContract(contract, endOfPeriod, financialQuery, billingDtos, cancellationToken);
            }
        }
    }

    private void ScanMonthlyBillingsForActiveContract(ContractGroup contract, DateTime? lastBillingEndDate,
        FinancialQuery financialQuery, List<BillingDto> billingDtos)
    {
        DateTime endOfPeriod;
        DateTime startOfPeriod;

        var today = DateTime.Today;

        var pc = new PersianCalendar();

        var period = contract.BillingPeriod;

        if (lastBillingEndDate.HasValue)
        {
            startOfPeriod = lastBillingEndDate.Value;

            endOfPeriod = pc.AddMonths(lastBillingEndDate.Value, 1);

            if (endOfPeriod < today)
            {
                var billingDto = new BillingDto
                {
                    CurrentPeriod = false,
                    ContractGroup = contract,
                    EndOfPeriod = endOfPeriod,
                    StartOfPeriod = startOfPeriod,
                    ContractIds = contract.ContractIds
                };

                billingDtos.Add(billingDto);

                var installmentQuery = merchantInstallmentRepository.CreateJobInstallmentQuery(startOfPeriod, endOfPeriod, contract.ContractIds);

                var financialDocumentQuery = financialDocumentRepository.CreateJobFinancialDocumentQuery(startOfPeriod, endOfPeriod, contract.ContractIds);

                financialQuery.BuildQueries(installmentQuery, financialDocumentQuery);

                ScanMonthlyBillingsForActiveContract(contract, endOfPeriod, financialQuery, billingDtos);
            }

            else if (endOfPeriod == today)
            {
                var billingDto = new BillingDto
                {
                    CurrentPeriod = true,
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
        }
        else
        {
            var year = pc.GetYear(today);
            var month = pc.GetMonth(today);
            var dayOfMonth = pc.GetDayOfMonth(today);
            var daysInMonth = pc.GetDaysInMonth(year, month);

            period = DateHelper.RegulateBillingPeriod(daysInMonth, period);

            var currentPeriod = period == dayOfMonth;

            if (currentPeriod)
            {
                endOfPeriod = today;

                startOfPeriod = pc.AddMonths(endOfPeriod, -1);

                var billingDto = new BillingDto
                {
                    CurrentPeriod = true,
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
        }
    }

    private async Task ScanMonthlyBillingsForDeactiveContract(ContractGroup contract, DateTime? lastBillingDueDate,
        FinancialQuery financialQuery, List<BillingDto> billingDtos, CancellationToken cancellationToken)
    {
        DateTime endOfPeriod;
        DateTime startOfPeriod;

        var pc = new PersianCalendar();

        var today = DateTime.Today;
        var endorsementDate = contract.EndorsementDate;

        var lastInstallmentDueDate = await merchantInstallmentRepository.GetLastInstallmentDueDate(contract.ContractIds, cancellationToken);

        if (!lastBillingDueDate.HasValue)
        {
            return;
        }

        if ((endorsementDate <= lastBillingDueDate) ||
            (lastBillingDueDate < endorsementDate && endorsementDate <= lastInstallmentDueDate))
        {
            startOfPeriod = lastBillingDueDate.Value;

            endOfPeriod = pc.AddMonths(lastBillingDueDate.Value, 1);

            if (endOfPeriod <= lastInstallmentDueDate)
            {
                var billingDto = new BillingDto
                {
                    CurrentPeriod = false,
                    ContractGroup = contract,
                    EndOfPeriod = endOfPeriod,
                    StartOfPeriod = startOfPeriod,
                    ContractIds = contract.ContractIds
                };

                billingDtos.Add(billingDto);

                var installmentQuery = merchantInstallmentRepository.CreateJobInstallmentQuery(startOfPeriod, endOfPeriod, contract.ContractIds);

                var financialDocumentQuery = financialDocumentRepository.CreateJobFinancialDocumentQuery(startOfPeriod, endOfPeriod, contract.ContractIds);

                financialQuery.BuildQueries(installmentQuery, financialDocumentQuery);

                await ScanMonthlyBillingsForDeactiveContract(contract, endOfPeriod, financialQuery, billingDtos, cancellationToken);
            }
            else
            {
                var currentPeriod = endOfPeriod == today;

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
        }

        else if (endorsementDate > lastInstallmentDueDate)
        {
            startOfPeriod = lastBillingDueDate.Value;

            endOfPeriod = pc.AddMonths(lastBillingDueDate.Value, 1);

            if (endOfPeriod <= endorsementDate)
            {
                var billingDto = new BillingDto
                {
                    CurrentPeriod = false,
                    ContractGroup = contract,
                    EndOfPeriod = endOfPeriod,
                    StartOfPeriod = startOfPeriod,
                    ContractIds = contract.ContractIds
                };

                billingDtos.Add(billingDto);

                var installmentQuery = merchantInstallmentRepository.CreateJobInstallmentQuery(startOfPeriod, endOfPeriod, contract.ContractIds);

                var financialDocumentQuery = financialDocumentRepository.CreateJobFinancialDocumentQuery(startOfPeriod, endOfPeriod, contract.ContractIds);

                financialQuery.BuildQueries(installmentQuery, financialDocumentQuery);

                await ScanMonthlyBillingsForDeactiveContract(contract, endOfPeriod, financialQuery, billingDtos, cancellationToken);
            }
        }
    }

    private async Task<List<NotSettledBilling>> OverdueExpiredBillings(CancellationToken cancellationToken)
    {
        var notAssignedBillings = await merchantBillingRepository.GetOverdueOrNotSettledBillings(cancellationToken);

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