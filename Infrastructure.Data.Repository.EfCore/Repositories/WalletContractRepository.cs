using Domain.Core.Entities.WalletContractAggregate;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

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

        public async Task<WalletContract> GetAsync(int id)
        {
            var contract = await _applicationDbContext.WalletContracts
                .FirstOrDefaultAsync(x => x.Id == id);

            return contract;
        }
    }

}
