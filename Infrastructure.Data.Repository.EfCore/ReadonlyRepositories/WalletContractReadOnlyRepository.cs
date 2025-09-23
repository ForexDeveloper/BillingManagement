using Application.Query.ReadOnlyRepositoryContracts;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;

namespace Infrastructure.Data.Repository.EfCore.ReadonlyRepositories
{
    public class WalletContractReadOnlyRepository : IWalletContractReadOnlyRepository
    {
        private readonly ReadonlyApplicationDbContext _readonlyApplicationDbContext;

        public WalletContractReadOnlyRepository(ReadonlyApplicationDbContext readonlyApplicationDbContext)
        {
            _readonlyApplicationDbContext = readonlyApplicationDbContext;
        }

        public async Task<int?> GetTenantIpgSettingIdAsync(int id, int tenantId)
        {
            //To-do
            var contract = await _readonlyApplicationDbContext.WalletContracts
                .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);

            return contract?.TenantIpgSettingId;
        }

    }
}
