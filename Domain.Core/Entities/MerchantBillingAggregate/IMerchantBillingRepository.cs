using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Domain.Core.Entities.MerchantBillingAggregate.ValueObjects;

namespace Domain.Core.Entities.MerchantBillingAggregate;

public interface IMerchantBillingRepository
{
    Task AddRangeAsync(IEnumerable<MerchantBilling> billings, CancellationToken cancellationToken);

    Task<List<NotSettledMerchantBilling>> GetNotSettledBillings(CancellationToken cancellationToken);
}