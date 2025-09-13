using Domain.Core.Entities.ProjectManegerAggregate;
using Domain.Core.UnitOfWorkContracts;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.EventBus.Events;
using Shared.Logging.Abstraction.Extensions;
using Shared.Logging.Abstraction.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;


namespace Application.Service.EventConsumers;

public class TenantProjectManagerAddedOrUpdatedEventConsumer : IConsumer<ImTenantProjectManagerAddedOrUpdatedEvent>
{
    private readonly IProjectManagerRepository _projectManagerRepository;
    private readonly IApplicationDbContextUnitOfWork _unitOfWork;
    public readonly ILogger<TenantProjectManagerAddedOrUpdatedEventConsumer> _logger;

    public TenantProjectManagerAddedOrUpdatedEventConsumer(ILogger<TenantProjectManagerAddedOrUpdatedEventConsumer> logger, IProjectManagerRepository projectManagerRepository,
        IApplicationDbContextUnitOfWork unitOfWork)
    {
        _projectManagerRepository = projectManagerRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ImTenantProjectManagerAddedOrUpdatedEvent> context)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        bool succeed = true;
        try
        {
            var projectManager = await _projectManagerRepository.GetAsync(context.Message.TenantId, context.Message.UserId);
            if (projectManager == null)
            {
                await CreateProjectManager(context);
                return;
            }

            await UpdateProjectManager(context, projectManager);
        }
        catch (Exception ex)
        {
            succeed = false;
            _logger.LogCritical(new LogStruct
            {
                Message = ex.Message,
                ServiceName = "TenantProjectManagerAddedOrUpdatedConsumer_Consume",
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
                ServiceName = "TenantProjectManagerAddedOrUpdatedConsumer_Consume",
                InputParams = context.Message,
                Results = succeed,
                ResponseTimeStopWatcher = stopWatch,
                Tags = LogMessageTag.EventBus,
            });
        }
    }

    private async Task CreateProjectManager(ConsumeContext<ImTenantProjectManagerAddedOrUpdatedEvent> context)
    {
        ProjectManager projectManager = new(context.Message.TenantId, context.Message.UserId, context.Message.FullName );
        if (context.Message.IsActive)
        {
            projectManager.Activate();
        }
        else
        {
            projectManager.Deactivate();
        }
        await _projectManagerRepository.AddAsync(projectManager);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task UpdateProjectManager(ConsumeContext<ImTenantProjectManagerAddedOrUpdatedEvent> context, ProjectManager projectManager)
    {
        projectManager.SetFullName(context.Message.FullName);

        if (context.Message.IsActive)
        {
            projectManager.Activate();
        }
        else
        {
            projectManager.Deactivate();
        }
        _projectManagerRepository.Update(projectManager);
        await _unitOfWork.SaveChangesAsync();
    }
}
