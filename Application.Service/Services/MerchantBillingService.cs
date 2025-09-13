using System;
using System.Linq;
using System.Threading;
using Domain.Core.Enums;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;
using Application.Service.Contracts;
using Domain.Core.UnitOfWorkContracts;
using Domain.Core.Entities.MerchantBillingAggregate;
using Domain.Core.Entities.MerchantInstallmentAggregate;
using Domain.Core.Entities.TenantMerchantContractAggregate;

namespace Application.Service.Services;

public sealed class MerchantBillingService(
    IApplicationDbContextUnitOfWork unitOfWork,
    IMerchantBillingRepository merchantBillingRepository,
    IMerchantInstallmentRepository merchantInstallmentRepository,
    ITenantMerchantContractRepository tenantMerchantContractRepository) : IMerchantBillingService
{
    public async Task CreateMerchantBilling(CancellationToken cancellationToken)
    {
        var billings = new List<MerchantBilling>();

        var notSettledBillings = await merchantBillingRepository.GetNotSettledBillings(cancellationToken);

        var noInstallmentContracts = await tenantMerchantContractRepository.GetNoInstallmentContracts(cancellationToken);

        var groupInstallments = await merchantInstallmentRepository.GetGroupIdentityContractInstallments(cancellationToken);

        foreach (var groupInstallment in groupInstallments)
        {
            var period = groupInstallment.BillingPeriod;
            var periodType = groupInstallment.BillingPeriodType;
            var tenantId = groupInstallment.FromBusinessIdentityId;
            var dailyOriginDate = groupInstallment.DailyBillingOriginDate;
            var toBusinessIdentity = groupInstallment.ToBusinessIdentityId;
            var fromBusinessIdentity = groupInstallment.FromBusinessIdentityId;

            if (!TryGetBillingRanges(periodType, period, dailyOriginDate, out var startDate, out var endDate)) continue;

            var amount = groupInstallment.Installments.Sum(p => p.Amount);
            var contractIds = groupInstallment.Installments.Select(q => q.TenantMerchantContractId);
            var previousPeriodRefundedPurchases = groupInstallment.Installments.Where(p => p.FinancialDocument.Type == FinancialDocumentType.Refund).Sum(p => p.Amount);
            var currentPeriodPurchaseTransactions = groupInstallment.Installments.Where(p => p.FinancialDocument.Type == FinancialDocumentType.Purchase).Sum(p => p.Amount);

            decimal previousDebitAmount = 0;

            MerchantBilling overdueBilling = null;

            //Always Expect to be only 1 billing
            var notSettledBilling = notSettledBillings.FirstOrDefault(p => p.ContractIds.Any(q => contractIds.Contains(q)));

            if (notSettledBilling != null)
            {
                previousDebitAmount = notSettledBilling.CalculateDebitAmount();

                if (notSettledBilling.Billing.Amount > 0 || previousDebitAmount > 0)
                {
                    notSettledBilling.Billing.Overdue();
                    overdueBilling = notSettledBilling.Billing;
                }
            }

            var billing = new MerchantBilling(tenantId, fromBusinessIdentity, toBusinessIdentity,
                BillingType.TenantToMerchant, amount, previousDebitAmount, 0, 0, startDate, endDate, 0, 0, 0, 0, 0,
                groupInstallment.Installments, overdueBilling);

            billings.Add(billing);
        }

        foreach (var noInstallmentContract in noInstallmentContracts)
        {
            var period = noInstallmentContract.BillingPeriod;
            var periodType = noInstallmentContract.BillingPeriodType;
            var dailyOriginDate = noInstallmentContract.DailyBillingOriginDate;
            var amount = noInstallmentContract.PeriodMinCommissionAmount ?? 0;

            if (!TryGetBillingRanges(periodType, period, dailyOriginDate, out var startDate, out var endDate)) continue;

            //Always Expect to be only 1 billing
            var notSettledBilling = notSettledBillings.FirstOrDefault(p => p.ContractIds.Any(id => id == noInstallmentContract.Id));

            decimal previousDebitAmount = 0;

            MerchantBilling overdueBilling = null;

            if (notSettledBilling != null)
            {
                previousDebitAmount = notSettledBilling.CalculateDebitAmount();

                if (notSettledBilling.Billing.Amount > 0 || previousDebitAmount > 0)
                {
                    notSettledBilling.Billing.Overdue();
                    overdueBilling = notSettledBilling.Billing;
                }
            }

            var billing = new MerchantBilling(noInstallmentContract.TenantId, noInstallmentContract.TenantId,
                noInstallmentContract.MerchantId, BillingType.TenantToMerchant, amount, previousDebitAmount, 0, 0,
                startDate, endDate, 0, 0, 0, 0, 0, overdueBilling);

            billings.Add(billing);
        }

        await merchantBillingRepository.AddRangeAsync(billings, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static bool TryGetBillingRanges(TimeInterval periodType, int period, DateTime? dailyOriginDate, out DateTime startDate, out DateTime endDate)
    {
        var pc = new PersianCalendar();

        startDate = DateTime.MinValue;
        endDate = DateTime.MaxValue;

        var today = DateTime.Today;

        var year = pc.GetYear(today);
        var month = pc.GetMonth(today);
        var day = pc.GetDayOfMonth(today);
        var dayOfWeek = pc.GetDayOfWeek(today);
        var originDay = dailyOriginDate.HasValue ? pc.GetDayOfMonth(dailyOriginDate.Value) : 0;

        switch (periodType)
        {
            case TimeInterval.Day:

                if (originDay == day)
                {
                    startDate = pc.AddDays(new DateTime(year, month, day, pc), -period);
                    endDate = pc.ToDateTime(year, month, day, 0, 0, 0, 0);
                    return true;
                }
                return false;

            case TimeInterval.Week:

                if ((DayOfWeek)period == dayOfWeek)
                {
                    startDate = pc.AddWeeks(new DateTime(year, month, day, pc), -1);
                    endDate = pc.ToDateTime(year, month, day, 0, 0, 0, 0);
                    return true;
                }
                return false;

            case TimeInterval.Month:

                if (period == day)
                {
                    startDate = pc.AddMonths(new DateTime(year, month, day, pc), -1);
                    endDate = pc.ToDateTime(year, month, day, 0, 0, 0, 0);
                    return true;
                }
                return false;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}