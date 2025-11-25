using Domain.Core.Entities.OrganizationAggregate;
using Domain.Core.UnitOfWorkContracts;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.EventBus.Events;
using Shared.Logging.Abstraction.Extensions;
using Shared.Logging.Abstraction.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Domain.Core.Enums;


namespace Application.Service.EventConsumers;

public class OrganizationAddedOrUpdatedEventConsumer : IConsumer<CmOrganizationAddedOrUpdatedEvent>
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IApplicationDbContextUnitOfWork _unitOfWork;
    public readonly ILogger<OrganizationAddedOrUpdatedEventConsumer> _logger;

    public OrganizationAddedOrUpdatedEventConsumer(ILogger<OrganizationAddedOrUpdatedEventConsumer> logger, IOrganizationRepository organizationRepository,
        IApplicationDbContextUnitOfWork unitOfWork)
    {
        _logger = logger;
        _organizationRepository = organizationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Consume(ConsumeContext<CmOrganizationAddedOrUpdatedEvent> context)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        bool succeed = true;
        try
        {
            var organization = await _organizationRepository.GetAsync(context.Message.Id);
            if (organization == null)
            {
                await CreateOrganization(context);
                return;
            }

            await UpdateOrganization(context, organization);
        }
        catch (Exception ex)
        {
            succeed = false;
            _logger.LogCritical(new LogStruct
            {
                Message = ex.Message,
                ServiceName = "OrganizationAddedOrUpdatedEventConsumer_Consume",
                InputParams = context.Message,
                Results = "",
                Exception = ex,
                ResponseTimeStopWatcher = stopWatch,
                Tags = LogMessageTag.EventBus
            });
            throw;
        }
        finally
        {
            _logger.LogTrace(new LogStruct
            {
                Message = "",
                ServiceName = "OrganizationAddedOrUpdatedEventConsumer_Consume",
                InputParams = context.Message,
                Results = succeed,
                ResponseTimeStopWatcher = stopWatch,
                Tags = LogMessageTag.EventBus,
            });
        }
    }

    private async Task CreateOrganization(ConsumeContext<CmOrganizationAddedOrUpdatedEvent> context)
    {
        Organization organization = new(context.Message.Id, context.Message.Title, context.Message.TenantId, 
            (OrganizationTypeEnum)context.Message.OrganizationType, 
            (OrganizationIdentityTypeEnum)context.Message.OrganizationIdentityType, 
            (CoWalletNameEnum?)context.Message.CoWalletName, context.Message.ParentId);

        await _organizationRepository.AddAsync(organization);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task UpdateOrganization(ConsumeContext<CmOrganizationAddedOrUpdatedEvent> context, Organization organization)
    {
        organization.Update(context.Message.TenantId, context.Message.Title, 
            (OrganizationTypeEnum)context.Message.OrganizationType,
            (OrganizationIdentityTypeEnum)context.Message.OrganizationIdentityType,
            (CoWalletNameEnum?)context.Message.CoWalletName, context.Message.ParentId);

        _organizationRepository.Update(organization);
        await _unitOfWork.SaveChangesAsync();
    }
}
