using Domain.Core.Entities.MerchantAggregate;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Enums;
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;

namespace Rest.Integration.Tests.Base
{
    public class BaseTestDataBuilder
    {
        private readonly SharedHostFixture _hostFixture;
        private ApplicationDbContext _context;

        public BaseTestDataBuilder(SharedHostFixture hostFixture)
        {
            _hostFixture = hostFixture;
            _context = _hostFixture.GetMainContext();
        }

        public async Task<Tenant> CreateTenant()
        {
            var tenant = new Tenant(1, "tt", "tpn", "tbn", "p1",true,true);
            
            _context.Add(tenant);
            _context.Tenants.Add(tenant);

            await _context.SaveChangesAsync();

            return tenant;
        }

        public async Task<Merchant> CreateMerchant(int tenantId)
        {
            var merchant = new Merchant(2, tenantId,"tt",(byte) IdentityTypeEnum.Individual,(byte) SaleType.Online,(byte) MerchantStatus.Active);

            _context.Add(merchant);
            _context.Merchants.Add(merchant);

            await _context.SaveChangesAsync();

            return merchant;
        }


    }
}
