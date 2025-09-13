using Application.Query.ReadOnlyRepositoryContracts;
using Application.Service.Contracts;
using Application.Service.EventConsumers;
using Application.Service.Services;
using Domain.Core.AggregateRoots.CategoryAggregate;
using Domain.Core.AggregateRoots.WalletConfigurationAggregate;
using Domain.Core.Entities.AccountAggregate;
using Domain.Core.Entities.BillingAggregate;
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
using Domain.Core.Entities.OrganizationAggregate;
using Domain.Core.Entities.PlanAggregate;
using Domain.Core.Entities.ProjectManegerAggregate;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Entities.TenantPlatformContractAggregate;
using Domain.Core.Entities.TransactionAggregate;
using Domain.Core.Entities.WalletAggregate;
using Domain.Core.Entities.WalletContractAggregate;
using Domain.Core.UnitOfWorkContracts;
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;
using Infrastructure.Data.Repository.EfCore.ReadonlyRepositories;
using Infrastructure.Data.Repository.EfCore.Repositories;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.EventBus.Configurations;
using Shared.EventBus.Contracts;
using Shared.EventBus.Services;
using Shared.Redis;
using System;

namespace Service.Worker
{
    internal static class ServiceRegistration
    {

        internal static void RegisterEventBus(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EventBusConfiguration>(configuration.GetSection("EventBusConfiguration"));
            services.AddScoped<IEventPublisherService, EventPublisherService>();

            services.AddMassTransit(x =>
            {
                x.AddConsumer<CategoryAddedOrUpdatedEventConsumer>()
                    .Endpoint(p => p.InstanceId = configuration["PublicAppConfiguration:ApplicationName"]);

                x.AddConsumer<RichTenantAddedOrUpdatedEventConsumer>()
                        .Endpoint(p => p.InstanceId = configuration["PublicAppConfiguration:ApplicationName"]);

                x.AddConsumer<TenantProjectManagerAddedOrUpdatedEventConsumer>()
                        .Endpoint(p => p.InstanceId = configuration["PublicAppConfiguration:ApplicationName"]);

                x.AddConsumer<OrganizationAddedOrUpdatedEventConsumer>()
                    .Endpoint(p => p.InstanceId = configuration["PublicAppConfiguration:ApplicationName"]);

                x.AddConsumer<CustomerAddedOrUpdatedEventConsumer>()
                    .Endpoint(p => p.InstanceId = configuration["PublicAppConfiguration:ApplicationName"]);

                x.AddConsumer<FinancierAddedOrUpdatedEventConsumer>()
                    .Endpoint(p => p.InstanceId = configuration["PublicAppConfiguration:ApplicationName"]);

                x.AddConsumer<FacilitatorAddedOrUpdatedEventConsumer>()
                    .Endpoint(p => p.InstanceId = configuration["PublicAppConfiguration:ApplicationName"]);

                x.AddConsumer<GuarantorAddedOrUpdatedEventConsumer>()
                    .Endpoint(p => p.InstanceId = configuration["PublicAppConfiguration:ApplicationName"]);

                x.AddConsumer<TenantIpgSettingsAddedOrUpdatedEventConsumer>()
                    .Endpoint(p => p.InstanceId = configuration["PublicAppConfiguration:ApplicationName"]);

                x.AddConsumer<CgmProcessAddedOrUpdatedEventConsumer>()
                    .Endpoint(p => p.InstanceId = configuration["PublicAppConfiguration:ApplicationName"]);

                x.AddConsumer<CgmProcessInstanceAddedEventConsumer>()
                    .Endpoint(p => p.InstanceId = configuration["PublicAppConfiguration:ApplicationName"]);

                x.AddConsumer<MerchantAddedOrUpdatedEventConsumer>()
                    .Endpoint(p => p.InstanceId = configuration["PublicAppConfiguration:ApplicationName"]);

                x.AddConsumer<MerchantBranchAddedOrUpdatedEventConsumer>()
                    .Endpoint(p => p.InstanceId = configuration["PublicAppConfiguration:ApplicationName"]);

                x.AddConsumer<CgmProcessInstanceTerminatedEventConsumer>()
                    .Endpoint(p => p.InstanceId = configuration["PublicAppConfiguration:ApplicationName"]);

                x.AddConsumer<CgmSettlementChequesProcessInstanceAddedEventConsumer>()
                    .Endpoint(p => p.InstanceId = configuration["PublicAppConfiguration:ApplicationName"]);

                x.AddConsumer<PmSettlementChequePaymentUpdatedEventConsumer>()
                    .Endpoint(p => p.InstanceId = configuration["PublicAppConfiguration:ApplicationName"]);

                x.AddConfigureEndpointsCallback((name, cfg) =>
                {
                    if (cfg is IRabbitMqReceiveEndpointConfigurator rmq)
                        rmq.SetQuorumQueue(3);
                });
                x.UsingRabbitMq((_, cfg) =>
                {
                    cfg.AutoStart = true;
                    cfg.Host(configuration["EventBusConfiguration:HostUrl"], "creditcore", h =>
                    {
                        h.Username(configuration["EventBusConfiguration:Username"]);
                        h.Password(configuration["EventBusConfiguration:Password"]);
                    });

                    cfg.UseMessageRetry(r =>
                    {
                        r.Intervals(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(50), TimeSpan.FromSeconds(200),
                            TimeSpan.FromSeconds(600), TimeSpan.FromSeconds(6000), TimeSpan.FromSeconds(60000));
                    });

                    cfg.ConfigureEndpoints(_);
                });
                x.SetKebabCaseEndpointNameFormatter();

            });
        }

        internal static void RegisterOutBoxServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IOutboxRepository, OutboxRepository>();
            services.AddScoped<IOutboxService, OutboxService>();
            services.AddScoped<ILoanWalletService, LoanWalletService>();
            services.AddScoped<ICashWalletService, CashWalletService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IBillingPaymentService, BillingPaymentService>();
        }
        internal static void RegisterUnitOfWorks(this IServiceCollection services)
        {
            services.AddScoped<IApplicationDbContextUnitOfWork, ApplicationDbContextUnitOfWork>();
        }
        internal static void RegisterRepositories(this IServiceCollection services)
        {
            services.AddScoped<IWalletConfigurationRepository, WalletConfigurationRepository>();
            services.AddScoped<ITenantRepository, TenantRepository>();
            services.AddScoped<IOrganizationRepository, OrganizationRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IFinancierRepository, FinancierRepository>();
            services.AddScoped<IFacilitatorRepository, FacilitatorRepository>();
            services.AddScoped<IGuarantorRepository, GuarantorRepository>();
            services.AddScoped<IProjectManagerRepository, ProjectManagerRepository>();
            services.AddScoped<IClosedloopRepository, ClosedloopRepository>();
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IInstallmentRepository, InstallmentRepository>();
            services.AddScoped<IBillingRepository, BillingRepository>();
            services.AddScoped<IMerchantBillingRepository, MerchantBillingRepository>();
            services.AddScoped<IMerchantInstallmentRepository, MerchantInstallmentRepository>();

            services.AddScoped<IWalletConfigurationReadOnlyRepository, WalletConfigurationReadOnlyRepository>();
            services.AddScoped<ITenantReadOnlyRepository, TenantReadOnlyRepository>();
            services.AddScoped<IProjectManagerReadOnlyRepository, ProjectManagerReadOnlyRepository>();
            services.AddScoped<IOrganizationReadOnlyRepository, OrganizationReadOnlyRepository>();
            services.AddScoped<IFinancierReadOnlyRepository, FinancierReadOnlyRepository>();
            services.AddScoped<IFacilitatorReadOnlyRepository, FacilitatorReadOnlyRepository>();
            services.AddScoped<IGuarantorReadOnlyRepository, GuarantorReadOnlyRepository>();
            services.AddScoped<ICustomerReadOnlyRepository, CustomerReadOnlyRepository>();
            services.AddScoped<IPlanReadOnlyRepository, PlanReadOnlyRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IMerchantRepository, MerchantRepository>();
            services.AddScoped<IWalletContractRepository, WalletContractRepository>();
            services.AddScoped<ITenantPlatformContractRepository, TenantPlatformContractRepository>();

            services.AddScoped<IWalletRepository, WalletRepository>();
            services.AddScoped<IPlanRepository, PlanRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<IInstallmentRepository, InstallmentRepository>();
            services.AddScoped<IFinancialDocumentRepository, FinancialDocumentRepository>();
            services.AddScoped<ICashOutRequestRepository, CashOutRequestRepository>();
        }
        internal static void RegisteRedisServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddRedisWithRedLockService(options =>
            {
                options.Configuration = configuration["RedisConfiguration:Host"] ?? throw new ArgumentNullException($"RedisConfiguration {options.Configuration} is not set");
                options.InstanceName = configuration["RedisConfiguration:InstanceName"] ?? throw new ArgumentNullException($"RedisInstanceName {options.Configuration} is not set");
            });
        }
        internal static void RegisterServices(this IServiceCollection services)
        {
            services.AddScoped<IBillingService, BillingService>();
            //services.AddScoped<IMerchantBillingService, MerchantBillingService>();
        }
    }
}
