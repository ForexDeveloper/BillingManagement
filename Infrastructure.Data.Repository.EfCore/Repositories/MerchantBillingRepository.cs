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
            .Where(p => (p.Status == BillingStatus.Overdue && p.HasAttachment == false) ||
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
            .Where(p => p.Status == BillingStatus.Settled && p.Amount < 0 && p.HasAttachment == false)
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

    public async Task<DateTime?> GetLastBillingDueDate(IEnumerable<int> contractIds, CancellationToken cancellationToken)
    {
        //return await applicationDbContext.MerchantBillings.AsNoTracking()
        //    .Where(p => p.ContractIds.Any(q => contractIds.Contains(q)))
        //    .MaxAsync(p => p.DueDate, cancellationToken);


        // resharper suggestion should not be applied !!!

        var lastBillingDueDate = await applicationDbContext.MerchantBillings.AsNoTracking()
            .OrderByDescending(p => p.DueDate)
            .Where(p => p.ContractIds.Any(q => contractIds.Contains(q)))
            .Select(p => p.DueDate)
            .FirstOrDefaultAsync(cancellationToken);

        return lastBillingDueDate == DateTime.MinValue ? null : lastBillingDueDate;
    }

    public async Task<bool> FindAnotherBillingOnEndOfPeriod(DateTime endOfPeriod, CancellationToken cancellationToken)
    {
        return await applicationDbContext.MerchantBillings
            .Where(p => p.DueDate == endOfPeriod)
            .AnyAsync(cancellationToken);
    }
}