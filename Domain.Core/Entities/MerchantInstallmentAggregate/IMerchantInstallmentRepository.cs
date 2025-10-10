using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Domain.Core.Entities.InstallmentAggregate.Dtos;
using Domain.Core.Entities.TenantMerchantContractAggregate;
using Domain.Core.Entities.TenantMerchantContractAggregate.Dtos;

namespace Domain.Core.Entities.MerchantInstallmentAggregate;

public interface IMerchantInstallmentRepository : IRepository<MerchantInstallment, long>
{
    IQueryable<MerchantInstallment> CreateJobInstallmentQuery(DateTime startOfPeriod,
        DateTime endOfPeriod, IEnumerable<int> contractIds);

    Task<InstallmentRange?> GetInstallmentRange(IEnumerable<int> contractIds, DateTime? lastBillingDueDate,
        CancellationToken cancellationToken);

    Task<Dictionary<ContractIdentifier, List<InstallmentDto>>> GetGroupContractInstallments(
        IQueryable<MerchantInstallment> query, CancellationToken cancellationToken);

    Task<IEnumerable<InstallmentDto>> GetInstallmentsInSpecificPeriod(ContractGroup contract, DateTime startOfPeriod,
        DateTime endOfPeriod, CancellationToken cancellationToken);

    Task<decimal> GetSumOfTieredTransactionsFromStartOfPeriod(TenantMerchantContract contract, DateTime startOfPeriod);

    Task<decimal> GetSumOfTieredTransactionsInSpecificPeriod(ContractGroup contract, DateTime startOfPeriod,
        DateTime endOfPeriod, CancellationToken cancellationToken);

    Task<decimal> GetSumOfTransactionsInSpecificPeriod(ContractGroup contract, DateTime startOfPeriod,
        DateTime endOfPeriod, CancellationToken cancellationToken);

    Task<decimal> GetSumOfCommissionsInSpecificPeriod(ContractGroup contract, DateTime startOfPeriod,
        DateTime endOfPeriod, CancellationToken cancellationToken);
}