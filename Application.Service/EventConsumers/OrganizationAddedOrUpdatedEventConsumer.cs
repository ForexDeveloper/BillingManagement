using System;
using MassTransit;
using Domain.Core.Enums;
using System.Diagnostics;
using Shared.EventBus.Events;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Domain.Core.UnitOfWorkContracts;
using Shared.Logging.Abstraction.Models;
using Shared.Logging.Abstraction.Extensions;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.OrganizationAggregate;

namespace Application.Service.EventConsumers;

public sealed class OrganizationAddedOrUpdatedEventConsumer(
    IApplicationDbContextUnitOfWork unitOfWork,
    IOrganizationRepository organizationRepository,
    ILogger<OrganizationAddedOrUpdatedEventConsumer> logger) : IConsumer<CmOrganizationAddedOrUpdatedEvent>
{
    public async Task Consume(ConsumeContext<CmOrganizationAddedOrUpdatedEvent> context)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        var succeed = true;
        const string SERVICE_NAME = $"{nameof(OrganizationAddedOrUpdatedEventConsumer)}_{nameof(Consume)}";
        try
        {
            var organization = await organizationRepository.GetAsync(context.Message.Id);

            if (organization == null)
            {
                await CreateOrganization(context);
            }
            else
            {
                await UpdateOrganization(context, organization);
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

    private async Task CreateOrganization(ConsumeContext<CmOrganizationAddedOrUpdatedEvent> context)
    {
        Organization organization = new(context.Message.Id, context.Message.Title, context.Message.TenantId,
            (OrganizationTypeEnum)context.Message.OrganizationType,
            (OrganizationIdentityTypeEnum)context.Message.OrganizationIdentityType,
            (CoWalletNameEnum?)context.Message.CoWalletName, context.Message.ParentId);

        await organizationRepository.AddAsync(organization);
        await unitOfWork.SaveChangesAsync();
    }

    private async Task UpdateOrganization(ConsumeContext<CmOrganizationAddedOrUpdatedEvent> context, Organization organization)
    {
        organization.Update(context.Message.TenantId, context.Message.Title,
            (OrganizationTypeEnum)context.Message.OrganizationType,
            (OrganizationIdentityTypeEnum)context.Message.OrganizationIdentityType,
            (CoWalletNameEnum?)context.Message.CoWalletName, context.Message.ParentId);

        organizationRepository.Update(organization);
        await unitOfWork.SaveChangesAsync();
    }
}
