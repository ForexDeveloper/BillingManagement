using Domain.Core.AggregateRoots.CategoryAggregate;
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

public class CategoryAddedOrUpdatedEventConsumer : IConsumer<CmCategoryAddedOrUpdatedEvent>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IApplicationDbContextUnitOfWork _unitOfWork;
    public readonly ILogger<CategoryAddedOrUpdatedEventConsumer> _logger;

    public CategoryAddedOrUpdatedEventConsumer(ILogger<CategoryAddedOrUpdatedEventConsumer> logger, ICategoryRepository categoryRepository,
        IApplicationDbContextUnitOfWork unitOfWork)
    {
        _logger = logger;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Consume(ConsumeContext<CmCategoryAddedOrUpdatedEvent> context)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        bool succeed = true;
        try
        {
            var category = await _categoryRepository.GetAsync(context.Message.Id);
            if (category == null)
            {
                await CreateCategory(context);
                return;
            }

            await UpdateCategory(context, category);
        }
        catch (Exception ex)
        {
            succeed = false;
            _logger.LogCritical(new LogStruct
            {
                Message = ex.Message,
                ServiceName = "CategoryAddedOrUpdatedEventConsumer_Consume",
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
                ServiceName = "CategoryAddedOrUpdatedEventConsumer_Consume",
                InputParams = context.Message,
                Results = succeed,
                ResponseTimeStopWatcher = stopWatch,
                Tags = LogMessageTag.EventBus,
            });
        }
    }

    private async Task CreateCategory(ConsumeContext<CmCategoryAddedOrUpdatedEvent> context)
    {
        var category = new Category(context.Message.Id, context.Message.Title, context.Message.ParentId);
        await _categoryRepository.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task UpdateCategory(ConsumeContext<CmCategoryAddedOrUpdatedEvent> context, Category category)
    {
        category.SetTitle(context.Message.Title);
        _categoryRepository.Update(category);
        await _unitOfWork.SaveChangesAsync();
    }
}
