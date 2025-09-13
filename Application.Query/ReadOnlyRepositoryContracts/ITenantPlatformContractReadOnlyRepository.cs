using Application.Query.Queries;
using Application.Query.QueryModels;
using Domain.Core.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Query.ReadOnlyRepositoryContracts;

public interface ITenantPlatformContractReadOnlyRepository
{
    Task<TenantPlatformContractsQueryModel> GetListAsync(GetTenantPlatformContractsQuery request);
    Task<List<TenantPlatformContractSummaryQueryModel>> GetActiveContractsAsync(int tenantId);
    Task<List<ProviderQueryModel>> GetProvidersByTenantIdAsync(ProviderType? type, int tenantId);
}
