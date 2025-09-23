using System;
using System.Linq;
using System.Threading;
using Domain.Core.Enums;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Domain.Core.Entities.B2bBillingAggregate.Dtos;
using Domain.Core.Entities.MerchantBillingAggregate;
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;

namespace Infrastructure.Data.Repository.EfCore.Repositories;

public sealed class MerchantBillingRepository(ApplicationDbContext applicationDbContext) : IMerchantBillingRepository
{
    public async Task<List<NotSettledBilling>> GetNotAssignedBillings(CancellationToken cancellationToken)
    {
        var billings = await applicationDbContext.MerchantBillings
            .Where(p => (p.Status == BillingStatus.Overdue && p.Children.Any() == false) ||
                        ((p.Status == BillingStatus.Issued || p.Status == BillingStatus.PartiallyPaid) && p.DueDate.AddDays(-1) < DateTime.Today))
            .Select(p => new NotSettledBilling
            {
                Billing = p,
                PaidAmount = p.Payments.Sum(q => q.Amount),
                Contract = applicationDbContext.TenantMerchantContracts.Include(q => q.Children)
                    .OrderByDescending(q => q.CreatedDateTime).FirstOrDefault(q => p.ContractIds.Contains(q.Id))
            })
            .ToListAsync(cancellationToken);

        foreach (var billing in billings)
        {
            billing.ChildContractId = GetChildContractId(billing);
        }

        return billings;
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

    public async Task AddRangeAsync(IEnumerable<MerchantBilling> billings, CancellationToken cancellationToken)
    {
        await applicationDbContext.MerchantBillings.AddRangeAsync(billings, cancellationToken);
    }

    private static int GetChildContractId(NotSettledBilling notSettledBilling)
    {
        var contract = notSettledBilling.Contract;

        if (contract.Children != null && contract.Children.Any())
        {
            foreach (var child in contract.Children)
            {
                if (child.Children != null && child.Children.Any())
                {
                    return GetChildContractId(notSettledBilling);
                }

                return child.Id;
            }
        }

        return contract.Id;
    }
}