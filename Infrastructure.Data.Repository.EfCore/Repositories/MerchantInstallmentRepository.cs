using System;
using System.Linq;
using System.Threading;
using Domain.Core.Enums;
using System.Threading.Tasks;
using System.Collections.Generic;
using Domain.Core.Entities.InstallmentAggregate.Dtos;
using Microsoft.EntityFrameworkCore;
using Domain.Core.Entities.MerchantInstallmentAggregate;
using Domain.Core.Entities.TenantMerchantContractAggregate;
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;
using Domain.Core.Entities.TenantMerchantContractAggregate.Dtos;

namespace Infrastructure.Data.Repository.EfCore.Repositories;

public sealed class MerchantInstallmentRepository(ApplicationDbContext applicationDbContext) : IMerchantInstallmentRepository
{
    public async Task AddRangeAsync(List<MerchantInstallment> installments)
    {
        await applicationDbContext.MerchantInstallments.AddRangeAsync(installments);
    }

    public IQueryable<MerchantInstallment> CreateJobInstallmentQuery(DateTime startOfPeriod,
        DateTime endOfPeriod, IEnumerable<int> contractIds)
    {
        return applicationDbContext.MerchantInstallments
            .Where(p => startOfPeriod <= p.DueDate && p.DueDate < endOfPeriod)
            .Where(p => contractIds.Contains(p.TenantMerchantContractId));
    }

    public async Task<InstallmentRange?> GetInstallmentRanges(IEnumerable<int> contractIds, DateTime? lastBillingDueDate, CancellationToken cancellationToken)
    {
        var query = applicationDbContext.MerchantInstallments.AsNoTracking()
            .Where(p => contractIds.Contains(p.TenantMerchantContractId));

        query = lastBillingDueDate.HasValue ?
            query.Where(p => lastBillingDueDate <= p.DueDate && p.DueDate < DateTime.Today) :
            query.Where(p => p.DueDate < DateTime.Today);

        var found = await query.AnyAsync(cancellationToken);

        if (found == false) return null;

        var minDueDate = await query.MinAsync(p => p.DueDate, cancellationToken);

        var maxDueDate = await query.MaxAsync(p => p.DueDate, cancellationToken);

        return new InstallmentRange(minDueDate, maxDueDate);
    }

    public async Task<Dictionary<ContractIdentifier, List<InstallmentDto>>> GetGroupContractInstallments(IQueryable<MerchantInstallment> query, CancellationToken cancellationToken)
    {
        if (query == null) return new Dictionary<ContractIdentifier, List<InstallmentDto>>();

        var nullDailyOriginDateQuery = query.Where(p => p.TenantMerchantContract.DailyBillingOriginDate == null);

        var notNullDailyOriginDateQuery = query.Where(p => p.TenantMerchantContract.DailyBillingOriginDate != null);

        var nullDailyOriginDateInstallments = await nullDailyOriginDateQuery.Select(p => new
        {
            p.Amount,
            p.DueDate,
            p.Commission,
            p.CashAmount,
            p.CreditAmount,
            p.PrepaymentAmount,
            p.TenantMerchantContract.TenantId,
            p.TenantMerchantContract.MerchantId,
            p.TenantMerchantContract.BillingPeriod,
            p.TenantMerchantContract.BillingPeriodType,
            p.TenantMerchantContract.CommissionCalculationType
        }).GroupBy(p => new ContractIdentifier(p.TenantId,
            p.MerchantId,
            p.BillingPeriod,
            p.BillingPeriodType,
            p.CommissionCalculationType))
        .ToDictionaryAsync(p => p.Key, p => p.Select(q => new InstallmentDto(q.Amount,
                q.CashAmount,
                q.CreditAmount,
                q.PrepaymentAmount,
                q.Commission,
                q.DueDate)).ToList(), cancellationToken);

        var notNullDailyOriginDateInstallments = await notNullDailyOriginDateQuery.Select(p => new
        {
            p.Amount,
            p.DueDate,
            p.Commission,
            p.CashAmount,
            p.CreditAmount,
            p.PrepaymentAmount,
            p.TenantMerchantContract.TenantId,
            p.TenantMerchantContract.MerchantId,
            p.TenantMerchantContract.BillingPeriod,
            p.TenantMerchantContract.BillingPeriodType,
            p.TenantMerchantContract.DailyBillingOriginDate,
            p.TenantMerchantContract.CommissionCalculationType
        }).GroupBy(p => new ContractIdentifier(p.TenantId,
            p.MerchantId,
            p.BillingPeriod,
            p.BillingPeriodType,
            p.DailyBillingOriginDate,
            p.CommissionCalculationType))
            .ToDictionaryAsync(p => p.Key, p => p.Select(q => new InstallmentDto(q.Amount,
                q.CashAmount,
                q.CreditAmount,
                q.PrepaymentAmount,
                q.Commission,
                q.DueDate)).ToList(), cancellationToken);


        return notNullDailyOriginDateInstallments.Concat(nullDailyOriginDateInstallments).ToDictionary(p => p.Key, p => p.Value);
    }

    public async Task<decimal> GetSumOfTransactionsOfCurrentPeriod(TenantMerchantContract contract, DateTime startOfPeriod)
    {
        var query = applicationDbContext.MerchantInstallments
            .Where(p => p.Type == InstallmentType.Installment &&
                        startOfPeriod <= p.DueDate && p.DueDate <= DateTime.Today &&
                        p.TenantMerchantContract.BillingPeriod == contract.BillingPeriod &&
                        p.TenantMerchantContract.BillingPeriodType == contract.BillingPeriodType &&
                        p.TenantMerchantContract.DailyBillingOriginDate == contract.DailyBillingOriginDate &&
                        p.TenantMerchantContract.CommissionCalculationType == contract.CommissionCalculationType &&
                        p.FromBusinessIdentityId == contract.TenantId && p.ToBusinessIdentityId == contract.MerchantId);

        IQueryable<decimal> amountsQuery = null;

        if (!contract.CommissionReferenceTypes.Any())
        {
            amountsQuery = query.Select(p => p.Amount);
        }

        foreach (var contractCommissionReferenceType in contract.CommissionReferenceTypes)
        {
            switch (contractCommissionReferenceType)
            {
                case CommissionReferenceType.CashAmount:

                    var cashAmountQuery = query.Select(p => p.CashAmount);

                    amountsQuery = amountsQuery != null ?
                        amountsQuery.Union(cashAmountQuery) : cashAmountQuery;

                    break;

                case CommissionReferenceType.CreditAmount:

                    var creditAmountQuery = query.Select(p => p.CreditAmount);

                    amountsQuery = amountsQuery != null ?
                        amountsQuery.Union(creditAmountQuery) : creditAmountQuery;

                    break;

                case CommissionReferenceType.PrepaymentAmount:

                    var prePaymentAmountQuery = query.Select(p => p.PrepaymentAmount);

                    amountsQuery = amountsQuery != null ?
                        amountsQuery.Union(prePaymentAmountQuery) : prePaymentAmountQuery;

                    break;

                case CommissionReferenceType.InterestAmount:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        amountsQuery ??= query.Select(p => p.Amount);

        return await amountsQuery.SumAsync(p => p);
    }
}