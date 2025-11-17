using Domain.Core.Entities.TenantAggregate;
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;
using IntegrationTest.Server;

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
            var tenant = new Tenant(1, "tt", "tpn", "tbn", "p1");
            
            _context.Add(tenant);
            _context.Tenants.Add(tenant);

            await _context.SaveChangesAsync();

            return tenant;
        }

     
    }
}
