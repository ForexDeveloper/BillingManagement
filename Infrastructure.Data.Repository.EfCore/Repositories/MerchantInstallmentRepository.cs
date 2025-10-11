using System;
using System.Linq;
using System.Threading;
using Domain.Core.Enums;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Domain.Core.Entities.InstallmentAggregate.Dtos;
using Domain.Core.Entities.MerchantInstallmentAggregate;
using Domain.Core.Entities.TenantMerchantContractAggregate;
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;
using Domain.Core.Entities.TenantMerchantContractAggregate.Dtos;

namespace Infrastructure.Data.Repository.EfCore.Repositories;

public sealed class MerchantInstallmentRepository(ApplicationDbContext applicationDbContext)
    : Repository<MerchantInstallment, long>(applicationDbContext), IMerchantInstallmentRepository
{
    private readonly ApplicationDbContext _applicationDbContext = applicationDbContext;

    public IQueryable<MerchantInstallment> CreateJobInstallmentQuery(DateTime startOfPeriod,
        DateTime endOfPeriod, IEnumerable<int> contractIds)
    {
        return _applicationDbContext.MerchantInstallments
            .Where(p => contractIds.Contains(p.TenantMerchantContractId))
            .Where(p => startOfPeriod <= p.DueDate && p.DueDate < endOfPeriod);
    }

    public async Task<InstallmentRange?> GetInstallmentRange(IEnumerable<int> contractIds, DateTime? lastBillingDueDate, CancellationToken cancellationToken)
    {
        var query = _applicationDbContext.MerchantInstallments.AsNoTracking()
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

    public async Task<Dictionary<ContractIdentifier, List<InstallmentDto>>> GetGroupContractInstallments(
        IQueryable<MerchantInstallment> query, CancellationToken cancellationToken)
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

    public async Task<IEnumerable<InstallmentDto>> GetInstallmentsInSpecificPeriod(IEnumerable<int> contractIds,
        DateTime startOfPeriod, DateTime endOfPeriod, CancellationToken cancellationToken)
    {
        return await _applicationDbContext.MerchantInstallments
            .Where(p => p.Type == InstallmentType.Purchase)
            .Where(p => contractIds.Contains(p.TenantMerchantContractId))
            .Where(p => startOfPeriod <= p.DueDate && p.DueDate < endOfPeriod)
            .Select(p => new InstallmentDto()
            {
                Amount = p.Amount,
                CashAmount = p.CashAmount,
                Commission = p.Commission,
                CreditAmount = p.CreditAmount,
                PrepaymentAmount = p.PrepaymentAmount
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetSumOfTieredTransactionsInSpecificPeriod(TenantMerchantContract contract,
        DateTime startOfPeriod, DateTime endOfPeriod)
    {
        var query = _applicationDbContext.MerchantInstallments
            .Where(p => p.Type == InstallmentType.Purchase &&
                        startOfPeriod <= p.DueDate && p.DueDate < endOfPeriod &&
                        p.TenantMerchantContract.TenantId == contract.TenantId &&
                        p.TenantMerchantContract.MerchantId == contract.MerchantId &&
                        p.TenantMerchantContract.BillingPeriod == contract.BillingPeriod &&
                        p.TenantMerchantContract.BillingPeriodType == contract.BillingPeriodType &&
                        p.TenantMerchantContract.DailyBillingOriginDate == contract.DailyBillingOriginDate &&
                        p.TenantMerchantContract.CommissionCalculationType == contract.CommissionCalculationType);

        var amountsQuery = GetSumOfTieredTransactionsQuery(contract.CommissionReferenceTypes, query);

        return await amountsQuery.SumAsync(p => p);
    }

    public async Task<decimal> GetSumOfTieredTransactionsInSpecificPeriod(ContractGroup contract,
        DateTime startOfPeriod, DateTime endOfPeriod, CancellationToken cancellationToken)
    {
        var query = _applicationDbContext.MerchantInstallments
            .Where(p => p.Type == InstallmentType.Purchase)
            .Where(p => startOfPeriod <= p.DueDate && p.DueDate < endOfPeriod)
            .Where(p => contract.ContractIds.Contains(p.TenantMerchantContractId));

        var amountsQuery = GetSumOfTieredTransactionsQuery(contract.CommissionReferenceTypes, query);

        return await amountsQuery.SumAsync(p => p, cancellationToken);
    }

    public async Task<decimal> GetSumOfTransactionsInSpecificPeriod(IEnumerable<int> contractIds, DateTime startOfPeriod,
        DateTime endOfPeriod, CancellationToken cancellationToken)
    {
        return await _applicationDbContext.MerchantInstallments
            .Where(p => p.Type == InstallmentType.Purchase)
            .Where(p => contractIds.Contains(p.TenantMerchantContractId))
            .Where(p => startOfPeriod <= p.DueDate && p.DueDate < endOfPeriod)
            .SumAsync(p => p.Amount, cancellationToken);
    }

    public async Task<decimal> GetSumOfCommissionsInSpecificPeriod(IEnumerable<int> contractIds, DateTime startOfPeriod,
        DateTime endOfPeriod, CancellationToken cancellationToken)
    {
        return await _applicationDbContext.MerchantInstallments
            .Where(p => p.Type == InstallmentType.Purchase)
            .Where(p => contractIds.Contains(p.TenantMerchantContractId))
            .Where(p => startOfPeriod <= p.DueDate && p.DueDate < endOfPeriod)
            .SumAsync(p => p.Commission, cancellationToken);
    }

    private static IQueryable<decimal> GetSumOfTieredTransactionsQuery(
        List<CommissionReferenceType> commissionReferenceTypes, IQueryable<MerchantInstallment> query)
    {
        IQueryable<decimal> amountsQuery = null;

        if (commissionReferenceTypes != null && commissionReferenceTypes.Any())
        {
            foreach (var commissionReferenceType in commissionReferenceTypes)
            {
                switch (commissionReferenceType)
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

                        break;

                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }

        amountsQuery ??= query.Select(p => p.Amount);

        return amountsQuery;
    }
}