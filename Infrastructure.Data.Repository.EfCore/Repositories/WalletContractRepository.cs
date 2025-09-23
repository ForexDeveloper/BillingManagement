using Domain.Core.Entities.WalletContractAggregate;
using Domain.Core.Enums;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;

namespace Infrastructure.Data.Repository.EfCore.Repositories
{
    public class WalletContractRepository : IWalletContractRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public WalletContractRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task AddAsync(WalletContract contract)
        {
            await _applicationDbContext.WalletContracts.AddAsync(contract);
        }

        public void Update(WalletContract contract)
        {
            _applicationDbContext.WalletContracts.Update(contract);
        }

        public void UpdateRange(List<WalletContract> walletContracts)
        {
            _applicationDbContext.WalletContracts.UpdateRange(walletContracts);
        }

        public async Task<WalletContract> GetAsync(int id)
        {
            var contract = await _applicationDbContext.WalletContracts
                .FirstOrDefaultAsync(x => x.Id == id);

            return contract;
        }

        public async Task<List<WalletContract>> GetByRootParentIdAsync(int rootParentId, WalletContractStatus status)
        {
            var contracts = await _applicationDbContext.WalletContracts.Where(x => (x.Id == rootParentId || x.RootParentId == rootParentId) && x.Status == WalletContractStatus.Active).ToListAsync();

            return contracts;
        }
    }

}
