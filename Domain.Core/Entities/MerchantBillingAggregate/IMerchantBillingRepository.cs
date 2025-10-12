using Domain.Core.Entities.BillingAggregate.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Core.Entities.MerchantBillingAggregate;

public interface IMerchantBillingRepository
{
    Task<MerchantBilling> GetByBillingIdAsync(long billingId);
    void Update(MerchantBilling billings);
    Task AddRangeAsync(IEnumerable<MerchantBilling> billings, CancellationToken cancellationToken);

    IQueryable<MerchantBilling> CreateJobBillingQuery(DateTime startOfPeriod, DateTime endOfPeriod);

    Task<List<NotSettledBilling>> GetOverdueOrNotSettledBillings(CancellationToken cancellationToken);

    Task<bool> FindInContractPeriodAsync(IQueryable<MerchantBilling> query, CancellationToken cancellation);

    Task<bool> FindInContractPeriodAsync(DateTime startOfPeriod, DateTime endOfPeriod, CancellationToken cancellationToken);
}