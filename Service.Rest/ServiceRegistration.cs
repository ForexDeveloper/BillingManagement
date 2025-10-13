using Application.Command.Base;
using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Service.Contracts;
using Application.Service.Dtos.FileManagers;
using Application.Service.Dtos.Shared;
using Application.Service.Encryptions;
using Application.Service.Services;
using Domain.Core.Entities.FacilitatorAggregate;
using Domain.Core.Entities.FinancialDocumentAggregate;
using Domain.Core.Entities.FinancierAggregate;
using Domain.Core.Entities.GuarantorAggregate;
using Domain.Core.Entities.MerchantAggregate;
using Domain.Core.Entities.MerchantBillingAggregate;
using Domain.Core.Entities.MerchantInstallmentAggregate;
using Domain.Core.Entities.OrganizationAggregate;
using Domain.Core.Entities.Providers;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Entities.TenantMerchantContractAggregate;
using Domain.Core.Entities.TenantPlatformContractAggregate;
using Domain.Core.Entities.WalletContractAggregate;
using Domain.Core.UnitOfWorkContracts;
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;
using Infrastructure.Data.Repository.EfCore.ReadonlyRepositories;
using Infrastructure.Data.Repository.EfCore.Repositories;
using Shared.EventBus.Contracts;
using Shared.EventBus.Services;
using Shared.IdentityServerProvider;
using Shared.MediatR;
using Shared.MinIO;
using Shared.MinIO.Contracts;
using Shared.Redis;
using System.Reflection;
using Domain.Core.Entities.BackgroundJobAggregate;
using Domain.Core.Entities.BillingAggregate;
using Domain.Core.Entities.InstallmentAggregate;

namespace Service.Rest
{
    internal static class ServiceRegistration
    {
        internal static void RegisterRepositories(this IServiceCollection services)
        {
            #region ReadOnlyRepositories
            services.AddScoped<IAttachmentReadOnlyRepository, AttachmentReadOnlyRepository>();
            services.AddScoped<IBillingReadOnlyRepository, BillingReadOnlyRepository>();
            services.AddScoped<IFinancierReadOnlyRepository, FinancierReadOnlyRepository>();
            services.AddScoped<IGuarantorReadOnlyRepository, GuarantorReadOnlyRepository>();
            services.AddScoped<IFacilitatorReadOnlyRepository, FacilitatorReadOnlyRepository>();
            services.AddScoped<IFinancialDocumentReadOnlyRepository, FinancialDocumentReadOnlyRepository>();
            services.AddScoped<IMerchantBillingReadOnlyRepository, MerchantBillingReadOnlyRepository>();
            services.AddScoped<IMerchantReadOnlyRepository, MerchantReadOnlyRepository>();
            services.AddScoped<IOrganizationReadOnlyRepository, OrganizationReadOnlyRepository>();
            services.AddScoped<IProviderReadOnlyRepository, ProviderReadOnlyRepository>();
            services.AddScoped<ITenantPlatformContractReadOnlyRepository, TenantPlatformContractReadOnlyRepository>();
            services.AddScoped<ITenantMerchantContractReadOnlyRepository, TenantMerchantContractReadOnlyRepository>();
            services.AddScoped<ITenantReadOnlyRepository, TenantReadOnlyRepository>();
            services.AddScoped<IWalletContractReadOnlyRepository, WalletContractReadOnlyRepository>();
            #endregion

            #region Repositories
            services.AddScoped<IAttachmentRepository, AttachmentRepository>();
            services.AddScoped<IFacilitatorRepository, FacilitatorRepository>();
            services.AddScoped<IFinancialDocumentRepository, FinancialDocumentRepository>();
            services.AddScoped<IFinancierRepository, FinancierRepository>();
            services.AddScoped<IGuarantorRepository, GuarantorRepository>();

            services.AddScoped<IBackgroundJobRepository, BackgroundJobRepository>();
            services.AddScoped<IBillingRepository, BillingRepository>();
            services.AddScoped<IInstallmentRepository, InstallmentRepository>();
            services.AddScoped<IMerchantBillingRepository, MerchantBillingRepository>();
            services.AddScoped<IMerchantInstallmentRepository, MerchantInstallmentRepository>();

            services.AddScoped<IMerchantRepository, MerchantRepository>();
            services.AddScoped<IOrganizationRepository, OrganizationRepository>();
            services.AddScoped<IProviderRepository, ProviderRepository>();
            services.AddScoped<ITenantMerchantContractRepository, TenantMerchantContractRepository>();
            services.AddScoped<ITenantPlatformContractRepository, TenantPlatformContractRepository>();
            services.AddScoped<ITenantRepository, TenantRepository>();
            services.AddScoped<IWalletContractRepository, WalletContractRepository>();
            #endregion
        }

        internal static void UploadFileConfigurationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<UploadFileConfiguration>(configuration.GetSection("UploadFileConfiguration"));
        }

        internal static void RegisterMinIoServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IAttachmentRepository, AttachmentRepository>();

            services.AddMinIOService(config =>
            {
                config.HostURL = configuration["MinIOConnection:HostURL"] ?? throw new ArgumentNullException($"MinIOConnection {config.HostURL} is not set");
                config.AccessKey = configuration["MinIOConnection:AccessKey"] ?? throw new ArgumentNullException($"MinIOConnection {config.AccessKey} is not set");
                config.SecretKey = configuration["MinIOConnection:SecretKey"] ?? throw new ArgumentNullException($"MinIOConnection {config.SecretKey} is not set");
            });
        }

        internal static void RegisterEncryptionServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IEncryptionService, EncryptionService>();
        }

        internal static void RegisterOutBoxServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IOutboxRepository, OutboxRepository>();
            services.AddScoped<IOutboxService, OutboxService>();
        }
        internal static void RegisterUnitOfWorks(this IServiceCollection services)
        {
            services.AddScoped<IApplicationDbContextUnitOfWork, ApplicationDbContextUnitOfWork>();
        }
        internal static void RegisterMediatorService(this IServiceCollection services)
        {
            services.AddMediatorService(options =>
            {
                options.SetAssemblies(new Assembly[]
                {
                    Assembly.GetAssembly(typeof(BaseCommandHandler)),
                    Assembly.GetAssembly(typeof(BaseQueryHandler))
                }); ;
                options.EnableAutoLogging = true;
                options.EnableAutoValidation = true;
            });
        }
        internal static void RegisterAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCustomAuthentication(options =>
            {
                options.IdpServer = new Uri(configuration["IDP:Server"]);
                options.ValidateAudience = false;
                options.ValidateIssuer = false;
                options.ValidateIssuerSigningKey = false;
            });
        }

        internal static void RegisterServices(this IServiceCollection services)
        {
            services.AddScoped<IBackgroundJobService, BackgroundJobService>();
            services.AddScoped<ITenantMerchantContractService, TenantMerchantContractService>();
            services.AddScoped<IAttachmentService, AttachmentService>();
            services.AddScoped<IMerchantBillingService, MerchantBillingService>();
            services.AddScoped<ITenantPlatformContractService, TenantPlatformContractService>();
            services.AddScoped<IWalletContractService, WalletContractService>();
            services.AddScoped<IMerchantBillingService, MerchantBillingService>();
            services.AddScoped<IMerchantInstallmentService, MerchantInstallmentService>();
        }

        internal static void RegisterPublicAppConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<PublicAppConfiguration>(configuration.GetSection(nameof(PublicAppConfiguration)));
        }

        internal static void RegisteRedisServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddRedisWithRedLockService(options =>
            {
                options.Configuration = configuration["RedisConfiguration:Host"] ?? throw new ArgumentNullException($"RedisConfiguration {options.Configuration} is not set");
                options.InstanceName = configuration["RedisConfiguration:InstanceName"] ?? throw new ArgumentNullException($"RedisInstanceName {options.Configuration} is not set");
            });
        }
    }
}
