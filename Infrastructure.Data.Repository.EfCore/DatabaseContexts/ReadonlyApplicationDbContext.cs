using Domain.Core.AggregateRoots.CategoryAggregate;
using Domain.Core.AggregateRoots.WalletConfigurationAggregate;
using Domain.Core.Entities.AccountAggregate;
using Domain.Core.Entities.BankAccountAggregate;
using Domain.Core.Entities.BillingAggregate;
using Domain.Core.Entities.BusinessEntity;
using Domain.Core.Entities.CashOutRequestAggregate;
using Domain.Core.Entities.ClosedloopAggregate;
using Domain.Core.Entities.CustomerAggregate;
using Domain.Core.Entities.FacilitatorAggregate;
using Domain.Core.Entities.FinancialDocumentAggregate;
using Domain.Core.Entities.FinancierAggregate;
using Domain.Core.Entities.GuarantorAggregate;
using Domain.Core.Entities.InstallmentAggregate;
using Domain.Core.Entities.MerchantAggregate;
using Domain.Core.Entities.MerchantBillingAggregate;
using Domain.Core.Entities.MerchantInstallmentAggregate;
using Domain.Core.Entities.Merchants;
using Domain.Core.Entities.OrganizationAggregate;
using Domain.Core.Entities.PlanAggregate;
using Domain.Core.Entities.ProjectManegerAggregate;
using Domain.Core.Entities.Providers;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Entities.TenantMerchantContractAggregate;
using Domain.Core.Entities.TenantPlatformContractAggregate;
using Domain.Core.Entities.TransactionAggregate;
using Domain.Core.Entities.WalletAggregate;
using Domain.Core.Entities.WalletContractAggregate;
using GeneticsBank.Entities.Models.EntityConfigurations;
using Infrastructure.Data.Repository.EfCore.EntityConfigurations;
using Infrastructure.Data.Repository.EfCore.Exceptions;
using Microsoft.EntityFrameworkCore;
using Shared.EventBus.Entities;
using Shared.MinIO.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repository.EfCore
{
    public class ReadonlyApplicationDbContext : DbContext
    {
        public ReadonlyApplicationDbContext(DbContextOptions<ReadonlyApplicationDbContext> options)
           : base(options)
        {

        }
        public DbSet<MerchantBranch> MerchantBranches { get; set; }
        public DbSet<PlanDetailInstallment> PlanDetailInstallments { get; set; }
        public DbSet<MerchantCategory> MerchantCategories { get; set; }
        public DbSet<PlanClosedloop> PlanClosedloops { get; set; }
        public DbSet<PlanDetail> PlanDetails { get; set; }
        public DbSet<Plan> Plans { get; set; }
        public DbSet<Merchant> Merchants { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<WalletConfiguration> WalletConfigurations { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<ProjectManager> ProjectManagers { get; set; }
        public DbSet<CurrencyType> CurrencyTypes { get; set; }

        public DbSet<OutboxEntity> Outboxes { get; set; }
        public DbSet<ClosedloopCategory> ClosedloopCategories { get; set; }
        public DbSet<ClosedloopMerchant> ClosedloopMerchants { get; set; }
        public DbSet<Closedloop> Closedloops { get; set; }
        public DbSet<TenantMerchantContract> TenantMerchantContracts { get; set; }
        public DbSet<Attachment> Attachment { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<WalletContract> WalletContracts { get; set; }
        public DbSet<WalletContractGuarantor> WalletContractGuarantors { get; set; }
        public DbSet<WalletContractFinancier> WalletContractFinanciers { get; set; }
        public DbSet<WalletContractFacilitator> WalletContractFacilitators { get; set; }
        public DbSet<WalletContractBusinessIdentity> WalletContractBusinessIdentities { get; set; }
        public DbSet<WalletContractPlan> WalletContractPlans { get; set; }
        public DbSet<TenantPlatformContract> TenantPlatformContracts { get; set; }
        public DbSet<TenantPlatformContractFacilitator> TenantPlatformContractFacilitators { get; set; }
        public DbSet<TenantPlatformContractProvider> TenantPlatformContractProviders { get; set; }
        public DbSet<Provider> Providers { get; set; }
        public DbSet<CustomerOrganization> CustomerOrganizations { get; set; }
        public DbSet<WalletContractRejectionReason> WalletContractRejectionReasons { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Financier> Financiers { get; set; }
        public DbSet<FinancierCreditFlowConfig> FinancierCreditFlowConfigs { get; set; }
        public DbSet<Facilitator> Facilitators { get; set; }
        public DbSet<Guarantor> Guarantors { get; set; }
        public DbSet<BusinessIdentity> BusinessIdentities { get; set; }
        public DbSet<TenantIpgSetting> TenantIpgSettings { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<LoanWallet> LoanWallets { get; set; }
        public DbSet<CashWallet> CashWallets { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Installment> Installments { get; set; }
        public DbSet<FinancialDocument> FinancialDocuments { get; set; }
        public DbSet<FinancialDocumentPayment> FinancialDocumentPayments { get; set; }
        public DbSet<Billing> Billings { get; set; }
        public DbSet<BillingPayment> BillingPayments { get; set; }
        public DbSet<BillingInstallment> BillingInstallments { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<CashOutRequest> CashOutRequests { get; set; }
        public DbSet<MerchantBilling> MerchantBillings { get; set; }
        public DbSet<MerchantInstallment> MerchantInstallments { get; set; }

        public override int SaveChanges()
        {
            throw new ReadonlyDbContextException();
        }
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            throw new ReadonlyDbContextException();
        }
        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            throw new ReadonlyDbContextException();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("Fc");

            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new CurrencyTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ProjectManagerConfiguration());
            modelBuilder.ApplyConfiguration(new OutboxEntityConfiguration());
            modelBuilder.ApplyConfiguration(new WalletConfigurationConfiguration());
            modelBuilder.ApplyConfiguration(new TenantConfiguration());
            modelBuilder.ApplyConfiguration(new OutboxEntityConfiguration());
            modelBuilder.ApplyConfiguration(new MerchantConfiguration());
            modelBuilder.ApplyConfiguration(new CategoryConfiguration());
            modelBuilder.ApplyConfiguration(new ClosedloopCategoryConfiguration());
            modelBuilder.ApplyConfiguration(new ClosedloopMerchantConfiguration());
            modelBuilder.ApplyConfiguration(new ClosedloopConfiguration());
            modelBuilder.ApplyConfiguration(new PlanConfiguration());
            modelBuilder.ApplyConfiguration(new PlanClosedloopConfiguration());
            modelBuilder.ApplyConfiguration(new TenantMerchantContractConfiguration());
            modelBuilder.ApplyConfiguration(new WalletContractConfiguration());
            modelBuilder.ApplyConfiguration(new WalletContractFinancierConfiguration());
            modelBuilder.ApplyConfiguration(new WalletContractGuarantorConfiguration());
            modelBuilder.ApplyConfiguration(new WalletContractFacilitatorConfiguration());
            modelBuilder.ApplyConfiguration(new WalletContractBusinessIdentityConfiguration());
            modelBuilder.ApplyConfiguration(new TenantPlatformContractConfiguration());
            modelBuilder.ApplyConfiguration(new TenantPlatformContractFacilitatorConfiguration());
            modelBuilder.ApplyConfiguration(new TenantPlatformContractProviderConfiguration());
            modelBuilder.ApplyConfiguration(new ProviderConfiguration());
            modelBuilder.ApplyConfiguration(new WalletContractPlanConfiguration());
            modelBuilder.ApplyConfiguration(new WalletContractRejectionReasonConfiguration());
            modelBuilder.ApplyConfiguration(new OrganizationConfiguration());
            modelBuilder.ApplyConfiguration(new FinancierConfiguration());
            modelBuilder.ApplyConfiguration(new FinancierCreditFlowConfigConfiguration());
            modelBuilder.ApplyConfiguration(new FacilitatorConfiguration());
            modelBuilder.ApplyConfiguration(new GuarantorConfiguration());
            modelBuilder.ApplyConfiguration(new BusinessIdentityConfiguration());
            modelBuilder.ApplyConfiguration(new CustomerConfiguration());
            modelBuilder.ApplyConfiguration(new AttachmentConfiguration());
            modelBuilder.ApplyConfiguration(new CustomerOrganizationConfiguration());
            modelBuilder.ApplyConfiguration(new TenantIpgSettingConfiguration());
            modelBuilder.ApplyConfiguration(new BusinessIdentityWalletConfiguration());
            modelBuilder.ApplyConfiguration(new MerchantCategoryConfiguration());

            modelBuilder.ApplyConfiguration(new TransactionConfiguration());
            modelBuilder.ApplyConfiguration(new AccountConfiguration());
            modelBuilder.ApplyConfiguration(new InstallmentConfiguration());
            modelBuilder.ApplyConfiguration(new FinancialDocumentConfiguration());
            modelBuilder.ApplyConfiguration(new FinancialDocumentPaymentConfiguration());
            modelBuilder.ApplyConfiguration(new BillingConfiguration());
            modelBuilder.ApplyConfiguration(new BillingPaymentConfiguration());
            modelBuilder.ApplyConfiguration(new BillingInstallmentConfiguration());

            modelBuilder.ApplyConfiguration(new PlanDetailConfiguration());
            modelBuilder.ApplyConfiguration(new PlanDetailInstallmentConfiguration());
            modelBuilder.ApplyConfiguration(new MerchantBranchConfiguration());
            modelBuilder.ApplyConfiguration(new LoanWalletConfiguration());
            modelBuilder.ApplyConfiguration(new CashWalletConfiguration());
            modelBuilder.ApplyConfiguration(new BankAccountConfiguration());
            modelBuilder.ApplyConfiguration(new CashOutRequestConfiguration());
            modelBuilder.ApplyConfiguration(new MerchantBillingConfiguration());
            modelBuilder.ApplyConfiguration(new MerchantInstallmentConfiguration());
        }

    }
}