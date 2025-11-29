using Domain.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Entities.MerchantAggregate;
using Domain.Core.Entities.FinancialDocumentAggregate;
using Domain.Core.Entities.TenantMerchantContractAggregate;
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

    public async Task<FinancialDocument> CreateFinancialDocument()
    {
        var contract = await CreateTenantMerchantContract();

        var financialDocument = new FinancialDocument(1,
            _hostFixture.Tenant.Id,
            _hostFixture.Merchant.Id,
            _hostFixture.Tenant.Id,
            100000000,
            40000000,
            30000000,
            30000000,
            FinancialDocumentType.Purchase,
            FinancialDocumentState.Verified,
            PaymentGatewayType.Ipg,
            null,
            null,
            contract.Id
        );

        await _context.FinancialDocuments.AddAsync(financialDocument);

        await _context.SaveChangesAsync();

        return financialDocument;
    }

    public async Task<TenantMerchantContract> CreateTenantMerchantContract()
    {
        var tenantMerchantContract = new TenantMerchantContract(_hostFixture.Tenant.Id,
            _hostFixture.Merchant.Id,
            "TN-MR12349456547",
            DateTime.Now,
            DateTime.Now.AddMonths(6),
            SettlementType.Installments,
            true,
            13,
            CommissionDeductionMethodType.DeductEquallyFromInstallments,
            null,
            [
                InterestReferenceType.CreditAmount, InterestReferenceType.CashAmount,
                InterestReferenceType.PrepaymentAmount
            ],
            TimeInterval.Day,
            1,
            DateTime.Now.AddDays(-100),
            0,
            PaymentMethodType.BankAccountDeposit,
            GuaranteeType.House,
            null,
            CommissionCalculationType.FixedAmount,
            140000,
            13,
            [
                CommissionReferenceType.CashAmount, CommissionReferenceType.CreditAmount,
                CommissionReferenceType.PrepaymentAmount
            ],
            10000,
            50000,
            100000,
            500000
        );

        await _context.TenantMerchantContracts.AddAsync(tenantMerchantContract);

        await _context.SaveChangesAsync();

        return tenantMerchantContract;
    }
}