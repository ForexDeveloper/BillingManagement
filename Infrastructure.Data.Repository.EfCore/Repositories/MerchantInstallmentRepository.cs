using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Domain.Core.Entities.MerchantInstallmentAggregate;
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
            .Where(p => p.BillingId.HasValue == false)
            .Where(p => startOfPeriod <= p.DueDate && p.DueDate < endOfPeriod)
            .Where(p => contractIds.Contains(p.TenantMerchantContractId));
    }

    public async Task<bool> FindInContractPeriodAsync(IQueryable<MerchantInstallment> query, CancellationToken cancellationToken)
    {
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<InstallmentDates?> GetInstallmentsDates(DateTime endOfPeriod, IEnumerable<int> contractIds, CancellationToken cancellationToken)
    {
        var query = applicationDbContext.MerchantInstallments
            .Where(p => p.DueDate < endOfPeriod)
            .Where(p => p.BillingId.HasValue == false)
            .Where(p => contractIds.Contains(p.TenantMerchantContractId));

        var found = await query.AnyAsync(cancellationToken);

        if (found == false) return null;

        var minDueDate = await query.MinAsync(p => p.DueDate, cancellationToken);

        var maxDueDate = await query.MaxAsync(p => p.DueDate, cancellationToken);

        return new InstallmentDates(minDueDate, maxDueDate);
    }

    public async Task<Dictionary<ContractIdentifier, List<MerchantInstallment>>> GetGroupContractInstallments(IQueryable<MerchantInstallment> query, CancellationToken cancellationToken)
    {
        return await query.Select(p => new
        {
            Installemnt = p,
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
            .ToDictionaryAsync(p => p.Key, p => p.Select(q => q.Installemnt).ToList(), cancellationToken);
    }
}