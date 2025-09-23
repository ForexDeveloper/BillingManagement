using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Domain.Core.Entities.B2bBillingAggregate.Dtos;

namespace Domain.Core.Entities.MerchantBillingAggregate;

public interface IMerchantBillingRepository
{
    Task AddRangeAsync(IEnumerable<MerchantBilling> billings, CancellationToken cancellationToken);

    Task<List<NotSettledBilling>> GetNotAssignedBillings(CancellationToken cancellationToken);

    IQueryable<MerchantBilling> CreateJobBillingQuery(DateTime startOfPeriod, DateTime endOfPeriod);

    Task<bool> FindInContractPeriodAsync(IQueryable<MerchantBilling> query, CancellationToken cancellation);

    Task<bool> FindInContractPeriodAsync(DateTime startOfPeriod, DateTime endOfPeriod, CancellationToken cancellationToken);
}