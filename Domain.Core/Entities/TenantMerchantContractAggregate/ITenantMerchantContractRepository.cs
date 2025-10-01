using Domain.Core.Entities.TenantMerchantContractAggregate.Dtos;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Core.Entities.TenantMerchantContractAggregate;

public interface ITenantMerchantContractRepository
{
    Task AddAsync(TenantMerchantContract tenantMerchantContract);

    void Update(TenantMerchantContract tenantMerchantContract);

    Task<TenantMerchantContract> GetAsync(int id);

    Task<bool> IsContractBelongToTenantAsync(int id, int tenantId);

    Task<bool> IsExistsActiveContractAsync(int tenantId, int merchantId, int? contractId = null);

    Task<bool> IsDuplicatedContractNumberAsync(string contractNumber);

    Task<TenantMerchantContract> GetActiveContractAsync(int tenantId, int merchantId);

    Task<bool> HasEndorsement(int contractId, int tenantId);

    Task<List<int>> GetContractIdsHasEndorsement(List<int> contractIds);

    Task<List<ContractGroup>> GetAllGroupContractAsync(CancellationToken cancellationToken);

    Task<List<ContractGroup>> GetCurrentGroupContractsAsync(CancellationToken cancellationToken);

    DateTime GetActiveContractStartOfPeriod(TenantMerchantContract contract);
}