using Application.Query.Queries;
using Application.Query.QueryModels;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.ReadOnlyRepositoryContracts;

public interface IPlanReadOnlyRepository
{
    Task<PlanQueryModel> GetByIdAsync(int id, int? tenantId = null);

    Task<List<PlanSimpleListQueryModel>> GetPlansSimpleList(int tenantId, CancellationToken cancellationToken);

    Task<GetPlanForGridQueryModel> GetListAsync(GetAllPlanQuery query);

    Task<bool> HasWalletContractPlanByPlanId(int id);

    Task<bool> HasPlanByWalletConfigurationIdAsync(int walletConfigurationId);

    Task<GetPlanInstallmentsQueryModel> GetAllInstallmentsAsync(GetPlanInstallmentsQuery query, CancellationToken cancellationToken);
}