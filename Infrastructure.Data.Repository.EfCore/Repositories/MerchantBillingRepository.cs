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

public sealed class MerchantBillingRepository(ApplicationDbContext applicationDbContext)
    : Repository<MerchantBilling, long>(applicationDbContext), IMerchantBillingRepository
{
    private readonly ApplicationDbContext _applicationDbContext = applicationDbContext;

    public async Task AddRangeAsync(IEnumerable<MerchantBilling> billings, CancellationToken cancellationToken)
    {
        await _applicationDbContext.MerchantBillings.AddRangeAsync(billings, cancellationToken);
    }

    public async Task<List<NotSettledBilling>> GetOverdueOrNotSettledBillings(CancellationToken cancellationToken)
    {
        var billings = await _applicationDbContext.MerchantBillings
            .Include(p => p.Payments)
            .Where(p => (p.Status == BillingStatus.Overdue && p.Transferred == false) ||
                        ((p.Status == BillingStatus.Issued || p.Status == BillingStatus.PartiallyPaid) && p.PaymentDeadlineDate < DateTime.Today))
            .Select(p => new NotSettledBilling
            {
                Billing = p,
                PaidAmount = p.Payments.Sum(q => q.Amount),
                ActiveContractId = _applicationDbContext.TenantMerchantContracts.Where(q =>
                    q.Status &&
                    p.Type == BillingType.TenantToMerchant &&
                    q.TenantId == p.FromBusinessIdentityId &&
                    q.MerchantId == p.ToBusinessIdentityId).Select(q => q.Id).FirstOrDefault()
            })
            .OrderByDescending(p => p.Billing.DueDate)
            .ToListAsync(cancellationToken);

        return billings;
    }

    public async Task<List<NegativeSettledBilling>> GetNegativeSettledBillings(CancellationToken cancellationToken)
    {
        var billings = await _applicationDbContext.MerchantBillings
            .Where(p => p.Status == BillingStatus.Settled && p.Amount < 0 && p.Transferred == false)
            .Select(p => new NegativeSettledBilling
            {
                Billing = p,
                ActiveContractId = _applicationDbContext.TenantMerchantContracts.Where(q =>
                    q.Status &&
                    p.Type == BillingType.TenantToMerchant &&
                    q.TenantId == p.FromBusinessIdentityId &&
                    q.MerchantId == p.ToBusinessIdentityId).Select(q => q.Id).FirstOrDefault()
            })
            .OrderByDescending(p => p.Billing.DueDate)
            .ToListAsync(cancellationToken);

        return billings;
    }

    public async Task<DateTime?> GetLastBillingDueDate(IEnumerable<int> contractIds, CancellationToken cancellationToken)
    {
        //return await applicationDbContext.MerchantBillings.AsNoTracking()
        //    .Where(p => p.ContractIds.Any(q => contractIds.Contains(q)))
        //    .MaxAsync(p => p.DueDate, cancellationToken);


        // resharper suggestion should not be applied !!!

        var lastBillingDueDate = await _applicationDbContext.MerchantBillings.AsNoTracking()
            .OrderByDescending(p => p.DueDate)
            .Where(p => p.ContractIds.Any(q => contractIds.Contains(q)))
            .Select(p => p.DueDate)
            .FirstOrDefaultAsync(cancellationToken);

        return lastBillingDueDate == DateTime.MinValue ? null : lastBillingDueDate;
    }

    public async Task<bool> FindAnotherBillingOnEndOfPeriod(int tenantId, int merchantId, DateTime endOfPeriod, CancellationToken cancellationToken)
    {
        return await _applicationDbContext.MerchantBillings
            .Where(p => p.FromBusinessIdentityId == tenantId &&
                        p.ToBusinessIdentityId == merchantId &&
                        p.DueDate == endOfPeriod)
            .AnyAsync(cancellationToken);
    }

    public override async Task<MerchantBilling> GetAsync(long id, CancellationToken? cancellationToken = null)
    {
        var merchantBilling = await _applicationDbContext.MerchantBillings.Include(mb => mb.Payments)
            .FirstOrDefaultAsync(mb => mb.Id == id);

        return merchantBilling;
    }
}