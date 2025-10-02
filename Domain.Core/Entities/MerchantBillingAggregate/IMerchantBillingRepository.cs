using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Domain.Core.Entities.BillingAggregate.Dtos;

namespace Domain.Core.Entities.MerchantBillingAggregate;

public interface IMerchantBillingRepository
{
    Task AddRangeAsync(IEnumerable<MerchantBilling> billings, CancellationToken cancellationToken);

    IQueryable<MerchantBilling> CreateJobBillingQuery(DateTime startOfPeriod, DateTime endOfPeriod);

    Task<List<NotSettledBilling>> GetOverdueOrNotSettledBillings(CancellationToken cancellationToken);

    Task<List<NegativeSettledBilling>> GetNegativeSettledBillings(CancellationToken cancellationToken);

    Task<DateTime> GetLastBillingEndDate(IEnumerable<int> contractIds, CancellationToken cancellationToken);

    Task<bool> FindInContractPeriodAsync(IQueryable<MerchantBilling> query, CancellationToken cancellation);

    Task<bool> FindInContractPeriodAsync(DateTime startOfPeriod, DateTime endOfPeriod, CancellationToken cancellationToken);
}