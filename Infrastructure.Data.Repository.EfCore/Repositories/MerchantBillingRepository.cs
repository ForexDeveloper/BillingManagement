using System;
using System.Linq;
using System.Threading;
using Domain.Core.Enums;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Domain.Core.Entities.MerchantBillingAggregate;
using Domain.Core.Entities.MerchantBillingAggregate.ValueObjects;
using Domain.Core.Entities.MerchantInstallmentAggregate;

namespace Infrastructure.Data.Repository.EfCore.Repositories;

public sealed class MerchantBillingRepository(ApplicationDbContext applicationDbContext) : IMerchantBillingRepository
{
    public async Task<List<NotSettledMerchantBilling>> GetNotSettledBillings(CancellationToken cancellationToken)
    {
        return await applicationDbContext.MerchantBillings
            .Where(p => (p.Status == BillingStatus.Issued || p.Status == BillingStatus.PartiallyPaid) && p.DueDate < DateTime.Today && !p.ParentId.HasValue)
            .Select(p => new NotSettledMerchantBilling
            {
                Billing = p,
                SumOfPayments = p.Payments.Sum(q => q.Amount),
                ContractIds = p.Installments.Cast<MerchantInstallment>().Select(q => q.TenantMerchantContractId)
            })
            .ToListAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<MerchantBilling> billings, CancellationToken cancellationToken)
    {
        await applicationDbContext.MerchantBillings.AddRangeAsync(billings, cancellationToken);
    }
}