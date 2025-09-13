using Domain.Core.AggregateRoots.CategoryAggregate;
using Domain.Core.Entities.AccountAggregate;
using Domain.Core.Entities.MerchantAggregate;
using Domain.Core.Enums;
using Domain.Core.UnitOfWorkContracts;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.EventBus.Events;
using Shared.Logging.Abstraction.Extensions;
using Shared.Logging.Abstraction.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;


namespace Application.Service.EventConsumers;

public class MerchantBranchAddedOrUpdatedEventConsumer : IConsumer<CmMerchantBranchAddedOrUpdatedEvent>
{
    private readonly IMerchantRepository _merchantRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IApplicationDbContextUnitOfWork _unitOfWork;
    public readonly ILogger<CategoryAddedOrUpdatedEventConsumer> _logger;

    public MerchantBranchAddedOrUpdatedEventConsumer(ILogger<CategoryAddedOrUpdatedEventConsumer> logger, ICategoryRepository categoryRepository,
        IApplicationDbContextUnitOfWork unitOfWork, IMerchantRepository merchantRepository,
        IAccountRepository accountRepository)
    {
        _logger = logger;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _merchantRepository = merchantRepository;
        _accountRepository = accountRepository;
    }

    public async Task Consume(ConsumeContext<CmMerchantBranchAddedOrUpdatedEvent> context)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        bool succeed = true;
        try
        {
            //var merchant = await _merchantRepository.GetByIdAsync(context.Message.MerchantId);
            var merchantBranch = await _merchantRepository.GetBranchAsync(context.Message.BranchId);
            if (merchantBranch == null)
            {
                await CreateMerchantBranch(context);
                return;
            }

            await UpdateMerchantBranch(context, merchantBranch);
        }
        catch (Exception ex)
        {
            succeed = false;
            _logger.LogCritical(new LogStruct
            {
                Message = ex.Message,
                ServiceName = "MerchantBranchAddedOrUpdatedEventConsumer_Consume",
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
                ServiceName = "MerchantBranchAddedOrUpdatedEventConsumer_Consume",
                InputParams = context.Message,
                Results = succeed,
                ResponseTimeStopWatcher = stopWatch,
                Tags = LogMessageTag.EventBus,
            });
        }
    }

    private async Task CreateMerchantBranch(ConsumeContext<CmMerchantBranchAddedOrUpdatedEvent> context)
    {
        var merchantBranch = new MerchantBranch(context.Message.BranchId, context.Message.MerchantId,
            context.Message.Title, context.Message.TerminalId);

        await _merchantRepository.AddBranchAsync(merchantBranch);
        //await CreateAccounts(context.Message.BranchId, tenantId);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task UpdateMerchantBranch(ConsumeContext<CmMerchantBranchAddedOrUpdatedEvent> context, MerchantBranch merchantBranch)
    {
        merchantBranch.Update(context.Message.Title);
        _merchantRepository.UpdateBranch(merchantBranch);
        //await CreateAccounts(context.Message.BranchId, tenantId);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task CreateAccounts(int merchantBranchId, int tenantId)
    {
        var merchantBranchAccounts = await _accountRepository.GetListAsync(merchantBranchId, tenantId);
        List<Account> accounts = [];

        if (!merchantBranchAccounts.Any(x => x.Type == AccountType.Purchase))
        {
            var account = new Account(merchantBranchId, tenantId, AccountType.Purchase, 0);
            accounts.Add(account);
        }

        if (!merchantBranchAccounts.Any(x => x.Type == AccountType.Bank))
        {
            var account = new Account(merchantBranchId, tenantId, AccountType.Bank, 0);
            accounts.Add(account);
        }

        if (accounts.Count > 0)
        {
            await _accountRepository.AddRangeAsync(accounts);
        }
    }
}
