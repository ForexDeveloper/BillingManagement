using Domain.Core.Entities.InstallmentAggregate.Dtos;
using Domain.Core.Entities.TenantMerchantContractAggregate.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Core.Entities.MerchantInstallmentAggregate;

public interface IMerchantInstallmentRepository : IRepository<MerchantInstallment, long>
{
    Task<InstallmentRange?> GetInstallmentRange(IEnumerable<int> contractIds, DateTime? lastBillingDueDate,
        CancellationToken cancellationToken);

    Task<Dictionary<ContractIdentifier, List<InstallmentDto>>> GetGroupContractInstallments(
        IQueryable<MerchantInstallment> query, CancellationToken cancellationToken);

    Task<IEnumerable<InstallmentDto>> GetInstallmentsInSpecificPeriod(IEnumerable<int> contractIds,
        DateTime startOfPeriod, DateTime endOfPeriod, CancellationToken cancellationToken);

    Task<decimal> GetSumOfTieredTransactionsInSpecificPeriod(ContractGroup contract, DateTime startOfPeriod,
        DateTime endOfPeriod, CancellationToken cancellationToken);

    Task<decimal> GetSumOfPurchaseTransactionsInSpecificPeriod(IEnumerable<int> contractIds, DateTime startOfPeriod,
        DateTime endOfPeriod, CancellationToken cancellationToken);

    Task<decimal> GetSumOfRefundTransactionsInSpecificPeriod(IEnumerable<int> contractIds, DateTime startOfPeriod,
      DateTime endOfPeriod, CancellationToken cancellationToken);

    Task<decimal> GetSumOfPurchaseCommissionsInSpecificPeriod(IEnumerable<int> contractIds, DateTime startOfPeriod,
        DateTime endOfPeriod, CancellationToken cancellationToken);

    Task<decimal> GetSumOfRefundCommissionsInSpecificPeriod(IEnumerable<int> contractIds, DateTime startOfPeriod,
        DateTime endOfPeriod, CancellationToken cancellationToken);
}