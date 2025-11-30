using Domain.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Entities.MerchantAggregate;
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;

namespace Rest.Integration.Tests.Base;

public class BaseTestDataBuilder
{
    private readonly ApplicationDbContext _context;
    private readonly SharedHostFixture _hostFixture;

    public BaseTestDataBuilder(SharedHostFixture hostFixture)
    {
        _hostFixture = hostFixture;
        _context = _hostFixture.GetMainContext();
    }

    public async Task<Tenant> CreateTenant()
    {
        const int TENANT_ID = 1;

        var tenant = await _context.Tenants.Where(p => p.Id == TENANT_ID).FirstOrDefaultAsync();

        if (tenant != null) return tenant;

        tenant = new Tenant(TENANT_ID, "tt", "tpn", "tbn", "p1", false, false);

        _context.Add(tenant);

        _context.Tenants.Add(tenant);

        await _context.SaveChangesAsync();

        return tenant;
    }

    public async Task<Merchant> CreateMerchant(int tenantId)
    {
        const int MERCHANT_ID = 2;

        var merchant = await _context.Merchants.Where(p => p.Id == MERCHANT_ID).FirstOrDefaultAsync();

        if (merchant != null) return merchant;

        merchant = new Merchant(MERCHANT_ID, tenantId, "tt", (byte)IdentityTypeEnum.Individual, (byte)SaleType.Online, (byte)MerchantStatus.Active);

        _context.Add(merchant);

        _context.Merchants.Add(merchant);

        await _context.SaveChangesAsync();

        return merchant;
    }
}