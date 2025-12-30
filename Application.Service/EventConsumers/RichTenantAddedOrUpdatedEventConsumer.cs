using System;
using MassTransit;
using Domain.Core.Enums;
using System.Diagnostics;
using Shared.EventBus.Events;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Domain.Core.UnitOfWorkContracts;
using Shared.Logging.Abstraction.Models;
using Domain.Core.Entities.TenantAggregate;
using Shared.Logging.Abstraction.Extensions;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.FinancierAggregate;
using Domain.Core.Entities.GuarantorAggregate;
using Domain.Core.Entities.FacilitatorAggregate;
using Domain.Core.Entities.OrganizationAggregate;

namespace Application.Service.EventConsumers;

public sealed class RichTenantAddedOrUpdatedEventConsumer(
    ITenantRepository tenantRepository,
    IFinancierRepository financierRepository,
    IGuarantorRepository guarantorRepository,
    IApplicationDbContextUnitOfWork unitOfWork,
    IFacilitatorRepository facilitatorRepository,
    IOrganizationRepository organizationRepository,
    ILogger<RichTenantAddedOrUpdatedEventConsumer> logger) : IConsumer<CmRichTenantAddedOrUpdatedEvent>
{
    public async Task Consume(ConsumeContext<CmRichTenantAddedOrUpdatedEvent> context)
    {
        var succeed = true;
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        const string SERVICE_NAME = $"{nameof(RichTenantAddedOrUpdatedEventConsumer)}_{nameof(Consume)}";
        try
        {
            var tenant = await tenantRepository.GetAsync(context.Message.Id);

            if (tenant == null)
            {
                await CreateRichTenant(context);
            }
            else
            {
                await UpdateRichTenant(context.Message, tenant);
            }
        }
        catch (Exception exception)
        {
            succeed = false;

            if (exception is IBusinessException)
            {
                logger.LogWarning(new LogStruct
                {
                    Exception = exception,
                    Results = string.Empty,
                    ServiceName = SERVICE_NAME,
                    Message = exception.Message,
                    InputParams = context.Message,
                    Tags = LogMessageTag.EventBus,
                    ResponseTimeStopWatcher = stopWatch
                });
            }
            else
            {
                logger.LogCritical(new LogStruct
                {
                    Exception = exception,
                    Results = string.Empty,
                    ServiceName = SERVICE_NAME,
                    Message = exception.Message,
                    InputParams = context.Message,
                    Tags = LogMessageTag.EventBus,
                    ResponseTimeStopWatcher = stopWatch
                });

                throw;
            }
        }
        finally
        {
            logger.LogTrace(new LogStruct
            {
                Results = succeed,
                Message = string.Empty,
                ServiceName = SERVICE_NAME,
                InputParams = context.Message,
                Tags = LogMessageTag.EventBus,
                ResponseTimeStopWatcher = stopWatch
            });
        }
    }

    private async Task CreateRichTenant(ConsumeContext<CmRichTenantAddedOrUpdatedEvent> context)
    {
        await CreateRichTenant(context.Message);
        await unitOfWork.SaveChangesAsync();
    }

    private async Task CreateRichTenant(CmRichTenantAddedOrUpdatedEvent message)
    {
        Tenant tenant = new(message.Id, message.Title, message.CreditProjectName, message.BrandName, message.InternalProjectManagerName,message.HasCoWallet,message.HasAnonymous);
        await tenantRepository.AddAsync(tenant);

        var organization = new Organization(message.OrganizationId, message.Title, message.Id, OrganizationTypeEnum.Classic, OrganizationIdentityTypeEnum.Identified);
        await organizationRepository.AddAsync(organization);

        var financier = new Financier(message.FinancierId, message.Title, message.Id, message.IdentityType, true);
        await financierRepository.AddAsync(financier);

        var facilitator = new Facilitator(message.FacilitatorId, message.Title, message.Id, message.IdentityType, true);
        await facilitatorRepository.AddAsync(facilitator);

        var guarantor = new Guarantor(message.GuarantorId, message.Title, message.Id, message.IdentityType, true);
        await guarantorRepository.AddAsync(guarantor);
    }

    private async Task UpdateRichTenant(CmRichTenantAddedOrUpdatedEvent message, Tenant tenant)
    {
        tenant.Update(message.Title, message.CreditProjectName, message.BrandName, message.InternalProjectManagerName,message.HasCoWallet, message.HasAnonymous);
        tenantRepository.Update(tenant);

        var organization = await organizationRepository.GetAsync(message.OrganizationId);
        var financier = await financierRepository.GetAsync(message.FinancierId);
        var facilitator = await facilitatorRepository.GetAsync(message.FacilitatorId);
        var guarantor = await guarantorRepository.GetAsync(message.GuarantorId);

        if (organization is not null && organization.Title != message.Title)
        {
            organization.SetTitle(message.Title);
            organizationRepository.Update(organization);
        }

        if (financier is not null && financier.Name != message.Title)
        {
            financier.SetName(message.Title);
            financierRepository.Update(financier);
        }

        if (facilitator is not null && facilitator.Name != message.Title)
        {
            facilitator.SetName(message.Title);
            facilitatorRepository.Update(facilitator);
        }

        if (guarantor is not null && guarantor.Name != message.Title)
        {
            guarantor.SetName(message.Title);
            guarantorRepository.Update(guarantor);
        }

        await unitOfWork.SaveChangesAsync();
    }
}