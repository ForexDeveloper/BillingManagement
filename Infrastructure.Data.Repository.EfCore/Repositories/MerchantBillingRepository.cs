using System;
using System.Linq;
using System.Threading;
using Domain.Core.Enums;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Domain.Core.Entities.BillingAggregate.Dtos;
using Domain.Core.Entities.MerchantBillingAggregate;
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;

namespace Infrastructure.Data.Repository.EfCore.Repositories;

public sealed class MerchantBillingRepository(ApplicationDbContext applicationDbContext) : IMerchantBillingRepository
{
    public async Task AddRangeAsync(IEnumerable<MerchantBilling> billings, CancellationToken cancellationToken)
    {
        await applicationDbContext.MerchantBillings.AddRangeAsync(billings, cancellationToken);
    }

    public async Task<List<NotSettledBilling>> GetOverdueOrNotSettledBillings(CancellationToken cancellationToken)
    {
        var billings = await applicationDbContext.MerchantBillings
            .Where(p => (p.Status == BillingStatus.Overdue && p.DebtorChildren.Any() == false) ||
                        ((p.Status == BillingStatus.Issued || p.Status == BillingStatus.PartiallyPaid) && p.DueDate.AddDays(1) < DateTime.Today))
            .Select(p => new NotSettledBilling
            {
                Billing = p,
                PaidAmount = p.Payments.Sum(q => q.Amount),
                ActiveContractId = applicationDbContext.TenantMerchantContracts.Where(q =>
                    q.Status &&
                    q.TenantId == p.FromBusinessIdentityId &&
                    q.MerchantId == p.ToBusinessIdentityId).Select(q => q.Id).FirstOrDefault()
            })
            .OrderByDescending(p => p.Billing.EndDate)
            .ToListAsync(cancellationToken);

        return billings;
    }

    public async Task<List<NegativeSettledBilling>> GetNegativeSettledBillings(CancellationToken cancellationToken)
    {
        return await applicationDbContext.MerchantBillings
            .Where(p => p.Status == BillingStatus.Settled && p.Amount < 0 && p.CreditorChildren.Any() == false)
            .Select(p => new NegativeSettledBilling
            {
                Billing = p,
                ActiveContractId = applicationDbContext.TenantMerchantContracts.Where(q =>
                    q.Status &&
                    q.TenantId == p.FromBusinessIdentityId &&
                    q.MerchantId == p.ToBusinessIdentityId).Select(q => q.Id).FirstOrDefault()
            })
            .OrderByDescending(p => p.Billing.EndDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<DateTime> GetLastBillingEndDate(IEnumerable<int> contractIds, CancellationToken cancellationToken)
    {
        return await applicationDbContext.MerchantBillings.AsNoTracking().OrderByDescending(p => p.EndDate)
            .Where(p => p.ContractIds.Any(contractIds.Contains)).Select(p => p.EndDate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public IQueryable<MerchantBilling> CreateJobBillingQuery(DateTime startOfPeriod, DateTime endOfPeriod)
    {
        return applicationDbContext.MerchantBillings.Where(p => startOfPeriod == p.StartDate && endOfPeriod == p.EndDate);
    }

    public async Task<bool> FindInContractPeriodAsync(IQueryable<MerchantBilling> query, CancellationToken cancellationToken)
    {
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> FindInContractPeriodAsync(DateTime startOfPeriod, DateTime endOfPeriod, CancellationToken cancellationToken)
    {
        return await applicationDbContext.MerchantBillings
            .Where(p => startOfPeriod == p.StartDate && endOfPeriod == p.EndDate).AnyAsync(cancellationToken);
    }
}