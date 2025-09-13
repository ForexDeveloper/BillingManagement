using Domain.Core.Entities.AccountAggregate;
using Domain.Core.Entities.MerchantAggregate;
using Domain.Core.Entities.Merchants;
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

public class MerchantAddedOrUpdatedEventConsumer : IConsumer<CmMerchantAddedOrUpdatedEvent>
{
    private readonly IMerchantRepository _merchantRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IApplicationDbContextUnitOfWork _unitOfWork;
    public readonly ILogger<MerchantAddedOrUpdatedEventConsumer> _logger;

    public MerchantAddedOrUpdatedEventConsumer(ILogger<MerchantAddedOrUpdatedEventConsumer> logger, IMerchantRepository merchantRepository,
        IAccountRepository accountRepository,
        IApplicationDbContextUnitOfWork unitOfWork)
    {
        _logger = logger;
        _merchantRepository = merchantRepository;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Consume(ConsumeContext<CmMerchantAddedOrUpdatedEvent> context)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        bool succeed = true;
        try
        {
            var merchant = await _merchantRepository.GetAsync(context.Message.MerchantId, context.Message.TenantId);
            if (merchant == null)
            {
                await CreateMerchant(context);
                return;
            }

            await UpdateMerchant(context, merchant);
        }
        catch (Exception ex)
        {
            succeed = false;
            _logger.LogCritical(new LogStruct
            {
                Message = ex.Message,
                ServiceName = "MerchantAddedOrUpdatedEventConsumer_Consume",
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
                ServiceName = "MerchantAddedOrUpdatedEventConsumer_Consume",
                InputParams = context.Message,
                Results = succeed,
                ResponseTimeStopWatcher = stopWatch,
                Tags = LogMessageTag.EventBus,
            });
        }
    }

    private async Task CreateMerchant(ConsumeContext<CmMerchantAddedOrUpdatedEvent> context)
    {
        var merchant = new Merchant(context.Message.MerchantId, context.Message.TenantId, context.Message.Title,
            context.Message.IdentityType, context.Message.SaleType, context.Message.Status);
        foreach (var item in context.Message.Categories)
        {
            merchant.SetCategories(new MerchantCategory(item));
        }

        merchant.SetMerchantBranches(new MerchantBranch(context.Message.BranchId, context.Message.MerchantId, context.Message.Title, context.Message.TerminalId, context.Message.IsMerchant));

        await _merchantRepository.AddAsync(merchant);
        await CreateAccounts(context.Message.MerchantId, context.Message.TenantId);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task UpdateMerchant(ConsumeContext<CmMerchantAddedOrUpdatedEvent> context, Merchant merchant)
    {
        var merchantCategories = merchant.MerchantCategories.Where(c => !context.Message.Categories.Contains(c.CategoryId)).ToList();
        foreach (var item in merchantCategories)
        {
            item.SetDeleted();
        }
        foreach (var category in context.Message.Categories)
        {
            if (!merchant.MerchantCategories.Any(c => c.CategoryId == category))
            {
                merchant.SetCategories(new MerchantCategory(category));
            }
        }
        merchant.SetMerchant(context.Message.Title, context.Message.IdentityType,
            context.Message.SaleType, context.Message.Status);

        _merchantRepository.Update(merchant);
        await CreateAccounts(context.Message.MerchantId, context.Message.TenantId);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task CreateAccounts(int merchantId, int tenantId)
    {
        var merchantAccounts = await _accountRepository.GetListAsync(merchantId, tenantId);
        List<Account> accounts = [];

        if (!merchantAccounts.Any(x => x.Type == AccountType.Purchase))
        {
            var account = new Account(merchantId, tenantId, AccountType.Purchase, 0);
            accounts.Add(account);
        }

        if (!merchantAccounts.Any(x => x.Type == AccountType.Bank))
        {
            var account = new Account(merchantId, tenantId, AccountType.Bank, 0);
            accounts.Add(account);
        }

        if (accounts.Count > 0)
        {
            await _accountRepository.AddRangeAsync(accounts);
        }
    }
}
