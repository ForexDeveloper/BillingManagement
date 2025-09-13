using Application.Service.Contracts;
using Application.Service.Helper;
using Domain.Core.Entities.BillingAggregate;
using Domain.Core.Entities.InstallmentAggregate;
using Domain.Core.Entities.PlanAggregate;
using Domain.Core.Entities.WalletAggregate;
using Domain.Core.Enums;
using Domain.Core.UnitOfWorkContracts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Service.Services;

public class BillingService : IBillingService
{
    public readonly IInstallmentRepository _installmentRepository;
    public readonly IWalletRepository _walletRepository;
    public readonly IBillingRepository _billingRepository;
    public readonly IPlanRepository _planRepository;
    public readonly ILogger<BillingService> _logger;
    private readonly IApplicationDbContextUnitOfWork _unitOfWork;

    public BillingService(ILogger<BillingService> logger, IInstallmentRepository installmentRepository,
        IApplicationDbContextUnitOfWork unitOfWork, IBillingRepository billingRepository, IWalletRepository walletRepository, IPlanRepository planRepository)
    {
        _logger = logger;
        _installmentRepository = installmentRepository;
        _unitOfWork = unitOfWork;
        _billingRepository = billingRepository;
        _walletRepository = walletRepository;
        _planRepository = planRepository;
    }

    public async Task CreateLoanWalletBillings()
    {
        var accountIds = await _installmentRepository.GetAccountIdsForCreateBilling();
        if (accountIds.Count == 0)
            return;

        foreach (var accountId in accountIds)
        {
            await CreateBillingsForAccount(accountId);
        }
    }

    private async Task CreateBillingsForAccount(int accountId)
    {
        var accountInstallments = await _installmentRepository.GetInstallmentsByAccountId(accountId);

        var parentInstallments = accountInstallments
            .Where(x => x.Type == InstallmentType.Installment)
            .ToList();

        if (parentInstallments.Count == 0)
            return;

        var billings = new List<Billing>();

        foreach (var parentInstallment in parentInstallments)
        {
            var billing = CreateBillingForInstallment(accountInstallments, parentInstallment);
            billings.Add(billing);
        }

        await _billingRepository.AddRangeAsync(billings);
        await _unitOfWork.SaveChangesAsync();
    }

    private Billing CreateBillingForInstallment(IEnumerable<Installment> accountInstallments, Installment parentInstallment)
    {
        var interestInstallment = accountInstallments
            .FirstOrDefault(x => x.Type == InstallmentType.Interest && x.ParentId == parentInstallment.Id);

        var interestAmount = 0m;
        if (interestInstallment != null)
        {
            interestAmount = interestInstallment.Amount;

            interestInstallment.UpdateHasBilling(true);
            interestInstallment.SetEditDateTime(DateTime.Now);

            _installmentRepository.Update(interestInstallment);
        }

        var billing = new Billing(
            parentInstallment.FromAccount,
            parentInstallment.TenantId,
            parentInstallment.TenantId,
            parentInstallment.StartDate,
            parentInstallment.DueDate,
            parentInstallment.Amount + interestAmount,
            gracePeriod: parentInstallment.GracePeriod,
            settlementType: parentInstallment.SettlementType
        );

        parentInstallment.UpdateHasBilling(true);
        parentInstallment.SetEditDateTime(DateTime.Now);

        _installmentRepository.Update(parentInstallment);

        billing.AddBillingInstallment(new BillingInstallment(parentInstallment.Id));

        return billing;
    }


    public async Task UpdateBillings()
    {
        var accountIds = await _installmentRepository.GetAccountIdsForCreateOrUpdateBilling();
        if (accountIds.Count == 0)
        {
            return;
        }

        var accountIdsPenaltyPercents = await _walletRepository.GetPenaltyPercentByAccountIdsAsync(accountIds);

        foreach (var accountId in accountIds)
        {
            var notCompletePaidInstallments = await _installmentRepository
                .GetNotCompletePaidInstallmentsByAccountId(accountId);

            var notCompletePaidParentInstallments = notCompletePaidInstallments
                .Where(x => x.Type == InstallmentType.Installment).ToList();

            decimal jobTotalPenalties = 0;

            foreach (var notCompletePaidParentInstallment in notCompletePaidParentInstallments)
            {
                var installments = notCompletePaidInstallments
                    .Where(x => x.Id == notCompletePaidParentInstallment.Id || x.ParentId == notCompletePaidParentInstallment.Id)
                    .ToList();

                var installmentRemainAmount = installments
                    .Where(x => x.State != InstallmentState.CompletePaid)
                    .Sum(x => x.Amount - x.PaidAmount);

                if (installmentRemainAmount > 0)
                {
                    var penaltyPercent = accountIdsPenaltyPercents[accountId] ?? 0;

                    var penaltyAmount = RoundHelper.RoundAmount((installmentRemainAmount * penaltyPercent) / 100);
                    int penaltyDays = 1;
                    if (notCompletePaidParentInstallment.LastPenaltyCalculationDate == null)
                    {
                        TimeSpan difference = DateTime.Now.Date.Subtract(notCompletePaidParentInstallment.DueDate.Date.AddDays(notCompletePaidParentInstallment.GracePeriod));
                        penaltyDays = difference.Days;
                    }
                    else
                    {
                        TimeSpan difference = DateTime.Now.Date.Subtract(notCompletePaidParentInstallment.LastPenaltyCalculationDate.Value.Date);
                        penaltyDays = difference.Days;
                    }

                    if (penaltyAmount > 0)
                    {
                        for (int i = 0; i < penaltyDays; i++)
                        {
                            var dueDate = DateTime.Now.Date.AddDays(-i);
                            await CreatePenaltyInstallment(notCompletePaidParentInstallment, penaltyAmount, dueDate);
                            jobTotalPenalties += penaltyAmount;
                        }
                    }

                    var notCompletePaidInterestInstallment = notCompletePaidInstallments.FirstOrDefault(x => x.Type == InstallmentType.Interest && x.ParentId == notCompletePaidParentInstallment.Id);
                    if (notCompletePaidInterestInstallment != null && notCompletePaidInterestInstallment.State != InstallmentState.CompletePaid)
                    {
                        notCompletePaidInterestInstallment.SetOverdue();
                        _installmentRepository.Update(notCompletePaidInterestInstallment);
                    }

                    if (notCompletePaidParentInstallment.State != InstallmentState.CompletePaid)
                    {
                        notCompletePaidParentInstallment.SetOverdue();
                        _installmentRepository.Update(notCompletePaidParentInstallment);
                    }
                }
            }

            var currentBilling = await _billingRepository.GetCurrentBillingByAccountIdAsync(accountId);

            if (currentBilling == null)
            {
                currentBilling = await _billingRepository.GetLastBillingByAccountIdAsync(accountId);

                //TODO Credit

                currentBilling.UpdatePreviousPenaltyAmount(jobTotalPenalties); //todo, maybe no need to add penalty for last billing(Saroo)
                currentBilling.SetEditDateTime(DateTime.Now);

                if (currentBilling.State != BillingState.Overdue)
                    currentBilling.UpdateState(BillingState.Overdue);

                _billingRepository.Update(currentBilling);
            }
            else
            {
                var notCompletePaidBilling = await _billingRepository.GetPreviousNotCompletePaidBillingAsync(accountId, currentBilling.Id);

                if (notCompletePaidBilling != null)
                {
                    var previousInstallmentIds = notCompletePaidBilling.BillingInstallments
                        .Where(x => x.Installment.State != InstallmentState.CompletePaid)
                        .Select(x => x.InstallmentId)
                        .ToList();

                    foreach (var previousInstallmentId in previousInstallmentIds)
                    {
                        if (!currentBilling.BillingInstallments.Any(x => x.InstallmentId == previousInstallmentId))
                        {
                            currentBilling.AddBillingInstallment(new BillingInstallment(previousInstallmentId, true));
                        }
                    }

                    decimal prevAmount = notCompletePaidBilling.Amount;
                    decimal prevDebitAmount = notCompletePaidBilling.PreviousDebitAmount;
                    decimal prevPenaltyAmount = notCompletePaidBilling.PreviousPenaltyAmount;

                    decimal prevCreditAmount = 0;

                    var prevCreditAndPaymentsSum = notCompletePaidBilling.PreviousCreditAmount +
                        notCompletePaidBilling.BillingPayments.Sum(x => x.Amount);

                    if (prevCreditAndPaymentsSum > 0)
                    {
                        if (prevCreditAndPaymentsSum >= notCompletePaidBilling.PreviousPenaltyAmount)
                        {
                            prevPenaltyAmount = 0;
                            prevCreditAndPaymentsSum -= notCompletePaidBilling.PreviousPenaltyAmount;
                        }
                        else
                        {
                            prevPenaltyAmount = prevPenaltyAmount - prevCreditAndPaymentsSum;
                            prevCreditAndPaymentsSum = 0;
                        }
                    }

                    if (prevCreditAndPaymentsSum > 0)
                    {
                        if (prevCreditAndPaymentsSum >= notCompletePaidBilling.PreviousDebitAmount)
                        {
                            prevDebitAmount = 0;
                            prevCreditAndPaymentsSum -= notCompletePaidBilling.PreviousDebitAmount;
                        }
                        else
                        {
                            prevDebitAmount = notCompletePaidBilling.PreviousDebitAmount - prevCreditAndPaymentsSum;
                            prevCreditAndPaymentsSum = 0;
                        }
                    }

                    if (prevCreditAndPaymentsSum > 0)
                    {
                        if (prevCreditAndPaymentsSum >= notCompletePaidBilling.Amount)
                        {
                            prevAmount = 0;
                            prevCreditAndPaymentsSum -= notCompletePaidBilling.Amount;
                        }
                        else
                        {
                            prevAmount = notCompletePaidBilling.Amount - prevCreditAndPaymentsSum;
                            prevCreditAndPaymentsSum = 0;
                        }
                    }

                    currentBilling.UpdatePreviousCreditAndDebitAndPenalty(prevAmount + prevDebitAmount,
                        prevCreditAmount,
                        prevPenaltyAmount + jobTotalPenalties);

                    currentBilling.SetEditDateTime(DateTime.Now);
                    _billingRepository.Update(currentBilling);

                    notCompletePaidBilling.UpdateState(BillingState.Overdue);
                    notCompletePaidBilling.SetEditDateTime(DateTime.Now);
                    _billingRepository.Update(notCompletePaidBilling);
                }
                else
                {
                    currentBilling.UpdatePreviousPenaltyAmount(jobTotalPenalties);
                    currentBilling.SetEditDateTime(DateTime.Now);
                    _billingRepository.Update(currentBilling);
                }
            }
        }
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task CreatePenaltyInstallment(Installment installment, decimal amount, DateTime date)
    {
        var penaltyInstallment = new Installment(installment.FromAccountId, installment.ToAccountId, installment.TenantId, amount, date, InstallmentType.Penalty, InstallmentCategory.CustomerToTenant, startDate: date);
        penaltyInstallment.SetParentId(installment.Id);
        await _installmentRepository.AddAsync(penaltyInstallment);
    }
}
