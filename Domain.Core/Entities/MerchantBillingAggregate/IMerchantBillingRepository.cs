using Domain.Core.Entities.BillingAggregate.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Core.Entities.MerchantBillingAggregate;

public interface IMerchantBillingRepository : IRepository<MerchantBilling, long>
{
    Task<MerchantBilling> GetByBillingIdAsync(long billingId);
    void Update(MerchantBilling billings);
    Task AddRangeAsync(IEnumerable<MerchantBilling> billings, CancellationToken cancellationToken);

    Task<List<NotSettledBilling>> GetOverdueOrNotSettledBillings(CancellationToken cancellationToken);

    Task<List<NegativeSettledBilling>> GetNegativeSettledBillings(CancellationToken cancellationToken);

    Task<DateTime?> GetLastBillingDueDate(IEnumerable<int> contractIds, CancellationToken cancellationToken);

    Task<bool> FindAnotherBillingOnEndOfPeriod(int tenantId, int merchantId, DateTime endOfPeriod, CancellationToken cancellationToken);
}