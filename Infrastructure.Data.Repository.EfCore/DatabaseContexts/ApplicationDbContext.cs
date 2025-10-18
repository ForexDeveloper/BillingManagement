using System.Linq;
using Shared.EventBus.Entities;
using Microsoft.EntityFrameworkCore;
using Domain.Core.Entities.Providers;
using Domain.Core.Entities.BusinessEntity;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Entities.BillingAggregate;
using Domain.Core.Entities.MerchantAggregate;
using Domain.Core.Entities.FinancierAggregate;
using Domain.Core.Entities.GuarantorAggregate;
using Domain.Core.Entities.FacilitatorAggregate;
using Domain.Core.Entities.InstallmentAggregate;
using Domain.Core.Entities.OrganizationAggregate;
using Domain.Core.Entities.BackgroundJobAggregate;
using Domain.Core.Entities.WalletContractAggregate;
using Domain.Core.Entities.BillingPaymentAggregate;
using Domain.Core.Entities.MerchantBillingAggregate;
using Domain.Core.Entities.FinancialDocumentAggregate;
using Domain.Core.Entities.MerchantInstallmentAggregate;
using Domain.Core.Entities.TenantMerchantContractAggregate;
using Domain.Core.Entities.TenantPlatformContractAggregate;
using Infrastructure.Data.Repository.EfCore.EntityConfigurations;

namespace Infrastructure.Data.Repository.EfCore.DatabaseContexts;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {

    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
       : base(options)
    {

    }

    public DbSet<MerchantBranch> MerchantBranches { get; set; }

    public DbSet<Merchant> Merchants { get; set; }

    public DbSet<Tenant> Tenants { get; set; }

    public DbSet<Organization> Organizations { get; set; }

    public DbSet<Financier> Financiers { get; set; }

    public DbSet<Facilitator> Facilitators { get; set; }

    public DbSet<Guarantor> Guarantors { get; set; }

    public DbSet<OutboxEntity> Outboxes { get; set; }

    public DbSet<TenantMerchantContract> TenantMerchantContracts { get; set; }

    public DbSet<BusinessIdentity> BusinessIdentities { get; set; }

    public DbSet<WalletContract> WalletContracts { get; set; }

    public DbSet<TenantPlatformContract> TenantPlatformContracts { get; set; }

    public DbSet<TenantPlatformContractFacilitator> TenantPlatformContractFacilitators { get; set; }

    public DbSet<TenantPlatformContractProvider> TenantPlatformContractProviders { get; set; }

    public DbSet<Provider> Providers { get; set; }

    public DbSet<TenantIpgSetting> TenantIpgSettings { get; set; }

    public DbSet<FinancialDocument> FinancialDocuments { get; set; }

    public DbSet<Billing> Billings { get; set; }

    public DbSet<BillingPayment> BillingPayments { get; set; }

    public DbSet<Installment> Installments { get; set; }

    public DbSet<MerchantBilling> MerchantBillings { get; set; }

    public DbSet<MerchantInstallment> MerchantInstallments { get; set; }

    public DbSet<BackgroundJob> BackgroundJobs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("Bill");

        var cascadeFKs = modelBuilder.Model.GetEntityTypes()
                        .SelectMany(t => t.GetForeignKeys())
                        .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

        foreach (var fk in cascadeFKs)
            fk.DeleteBehavior = DeleteBehavior.Restrict;


        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new TenantConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxEntityConfiguration());
        modelBuilder.ApplyConfiguration(new MerchantConfiguration());
        modelBuilder.ApplyConfiguration(new AttachmentConfiguration());
        modelBuilder.ApplyConfiguration(new OrganizationConfiguration());
        modelBuilder.ApplyConfiguration(new FinancierConfiguration());
        modelBuilder.ApplyConfiguration(new FacilitatorConfiguration());
        modelBuilder.ApplyConfiguration(new GuarantorConfiguration());
        modelBuilder.ApplyConfiguration(new BusinessIdentityConfiguration());
        modelBuilder.ApplyConfiguration(new TenantMerchantContractConfiguration());
        modelBuilder.ApplyConfiguration(new TenantPlatformContractConfiguration());
        modelBuilder.ApplyConfiguration(new TenantPlatformContractFacilitatorConfiguration());
        modelBuilder.ApplyConfiguration(new TenantPlatformContractProviderConfiguration());
        modelBuilder.ApplyConfiguration(new WalletContractConfiguration());
        modelBuilder.ApplyConfiguration(new WalletContractFinancierConfiguration());
        modelBuilder.ApplyConfiguration(new WalletContractFacilitatorConfiguration());
        modelBuilder.ApplyConfiguration(new WalletContractGuarantorConfiguration());
        modelBuilder.ApplyConfiguration(new ProviderConfiguration());
        modelBuilder.ApplyConfiguration(new TenantIpgSettingConfiguration());
        modelBuilder.ApplyConfiguration(new FinancialDocumentConfiguration());
        modelBuilder.ApplyConfiguration(new MerchantBranchConfiguration());
        modelBuilder.ApplyConfiguration(new BillingConfiguration());
        modelBuilder.ApplyConfiguration(new BillingPaymentConfiguration());
        modelBuilder.ApplyConfiguration(new InstallmentConfiguration());
        modelBuilder.ApplyConfiguration(new MerchantBillingConfiguration());
        modelBuilder.ApplyConfiguration(new MerchantInstallmentConfiguration());
        modelBuilder.ApplyConfiguration(new BackgroundJobConfiguration());
    }
}