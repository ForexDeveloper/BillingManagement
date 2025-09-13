using Domain.Base;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Core.AggregateRoots.WalletConfigurationAggregate
{
    public interface IWalletConfigurationRepository
    {
        Task AddAsync(WalletConfiguration WalletConfiguration);
        void Update(WalletConfiguration WalletConfiguration);
        Task<WalletConfiguration> GetByIdAsync(int id);
        Task<bool> CheckWalletConfigurationByTenantIdAsync(int id ,int tenantId);
        Task<WalletConfiguration> GetByTenantIdAsync(int id, int? tenantId=null);
        Task<bool> HasPlanAsync(int id);
    }
}