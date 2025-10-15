using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Domain.Core.Entities.InstallmentAggregate.Dtos;
using Domain.Core.Entities.TenantMerchantContractAggregate.Dtos;

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

    Task<decimal> GetSumOfTransactionsInSpecificPeriod(IEnumerable<int> contractIds, DateTime startOfPeriod,
        DateTime endOfPeriod, CancellationToken cancellationToken);

    Task<decimal> GetSumOfCommissionsInSpecificPeriod(IEnumerable<int> contractIds, DateTime startOfPeriod,
        DateTime endOfPeriod, CancellationToken cancellationToken);
}