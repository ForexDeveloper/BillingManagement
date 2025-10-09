using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Domain.Core.Entities.InstallmentAggregate.Dtos;
using Domain.Core.Entities.TenantMerchantContractAggregate;
using Domain.Core.Entities.TenantMerchantContractAggregate.Dtos;

namespace Domain.Core.Entities.MerchantInstallmentAggregate;

public interface IMerchantInstallmentRepository
{
    Task AddRangeAsync(List<MerchantInstallment> installments);

    IQueryable<MerchantInstallment> CreateJobInstallmentQuery(DateTime startOfPeriod,
        DateTime endOfPeriod, IEnumerable<int> contractIds);

    Task<InstallmentRange?> GetInstallmentRanges(IEnumerable<int> contractIds, DateTime? lastBillingDueDate,
        CancellationToken cancellationToken);

    Task<Dictionary<ContractIdentifier, List<InstallmentDto>>> GetGroupContractInstallments(IQueryable<MerchantInstallment> query, CancellationToken cancellationToken);

    Task<decimal> GetSumOfTransactionsOfCurrentPeriod(TenantMerchantContract contract, DateTime startOfPeriod);
}