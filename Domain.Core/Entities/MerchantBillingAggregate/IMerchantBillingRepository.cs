using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Domain.Core.Entities.BillingAggregate.Dtos;

namespace Domain.Core.Entities.MerchantBillingAggregate;

public interface IMerchantBillingRepository : IRepository<MerchantBilling, long>
{
    Task AddRangeAsync(IEnumerable<MerchantBilling> billings, CancellationToken cancellationToken);

    Task<List<NotSettledBilling>> GetOverdueOrNotSettledBillings(CancellationToken cancellationToken);

    Task<List<NegativeSettledBilling>> GetNegativeSettledBillings(CancellationToken cancellationToken);

    Task<DateTime?> GetLastBillingDueDate(IEnumerable<int> contractIds, CancellationToken cancellationToken);

    Task<bool> FindAnotherBillingOnEndOfPeriod(DateTime endOfPeriod, CancellationToken cancellationToken);
}