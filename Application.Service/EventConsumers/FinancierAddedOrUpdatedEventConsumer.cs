using Domain.Core.Entities.AccountAggregate;
using Domain.Core.Entities.FinancierAggregate;
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

public class FinancierAddedOrUpdatedEventConsumer : IConsumer<CmFinancierAddedOrUpdatedEvent>
{
    private readonly IFinancierRepository _financierRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IApplicationDbContextUnitOfWork _unitOfWork;
    public readonly ILogger<FinancierAddedOrUpdatedEventConsumer> _logger;

    public FinancierAddedOrUpdatedEventConsumer(ILogger<FinancierAddedOrUpdatedEventConsumer> logger, IFinancierRepository financierRepository,
        IAccountRepository accountRepository,
        IApplicationDbContextUnitOfWork unitOfWork)
    {
        _logger = logger;
        _financierRepository = financierRepository;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Consume(ConsumeContext<CmFinancierAddedOrUpdatedEvent> context)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        bool succeed = true;
        try
        {
            var financier = await _financierRepository.GetAsync(context.Message.Id);
            if (financier == null)
            {
                await CreateFinancier(context);
                return;
            }

            await UpdateFinancier(context, financier);
        }
        catch (Exception ex)
        {
            succeed = false;
            _logger.LogCritical(new LogStruct
            {
                Message = ex.Message,
                ServiceName = "FinancierAddedOrUpdatedEventConsumer_Consume",
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
                ServiceName = "FinancierAddedOrUpdatedEventConsumer_Consume",
                InputParams = context.Message,
                Results = succeed,
                ResponseTimeStopWatcher = stopWatch,
                Tags = LogMessageTag.EventBus,
            });
        }
    }

    private async Task CreateFinancier(ConsumeContext<CmFinancierAddedOrUpdatedEvent> context)
    {
        Financier financier = new(context.Message.Id, context.Message.Name, context.Message.TenantId, context.Message.Type, context.Message.IsTenant);

        if (context.Message.CreditFlowConfigs != null && context.Message.CreditFlowConfigs.Any())
        {
            foreach (var creditFlowConfig in context.Message.CreditFlowConfigs)
            {
                financier.AddCreditFlowConfig(new FinancierCreditFlowConfig(creditFlowConfig.Id, creditFlowConfig.ProductCode, creditFlowConfig.ClientId, creditFlowConfig.ClientSecret, creditFlowConfig.Type));
            }
        }
        await _financierRepository.AddAsync(financier);
        await CreateAccounts(context.Message.Id, context.Message.TenantId);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task UpdateFinancier(ConsumeContext<CmFinancierAddedOrUpdatedEvent> context, Financier financier)
    {
        financier.Update(context.Message.Name, context.Message.TenantId, context.Message.Type);

        if ((context.Message.CreditFlowConfigs is null || !context.Message.CreditFlowConfigs.Any()) && financier.CreditFlowConfigs.Any())
        {
            foreach (var creditFlowConfig in financier.CreditFlowConfigs)
            {
                creditFlowConfig.SetDeleted();
            }
        }
        else if (context.Message.CreditFlowConfigs != null)
        {

            var toBeDeletedCreditFlowConfigs = financier.CreditFlowConfigs
                .Where(c => !context.Message.CreditFlowConfigs.Select(x => x.Id).Contains(c.Id));


            foreach (var toBeDeletedCreditFlowConfig in toBeDeletedCreditFlowConfigs)
            {
                toBeDeletedCreditFlowConfig.SetDeleted();
            }

            var toBeEditedCreditFlowConfigs = context.Message.CreditFlowConfigs.Where(c => financier.CreditFlowConfigs.Select(s => s.Id).Contains(c.Id));

            foreach (var item in toBeEditedCreditFlowConfigs)
            {
                var creditFlowConfig = financier.CreditFlowConfigs.First(c => c.Id == item.Id);
                creditFlowConfig.SetFinancierCreditFlowConfig(item.ProductCode, item.ClientId, item.ClientSecret);
            }

            var toBeAddedCreditFlowConfigs = context.Message.CreditFlowConfigs.Where(c => !financier.CreditFlowConfigs.Select(s => s.Id).Contains(c.Id));

            foreach (var creditFlowConfig in toBeAddedCreditFlowConfigs)
            {
                financier.AddCreditFlowConfig(new FinancierCreditFlowConfig(creditFlowConfig.Id, creditFlowConfig.ProductCode, creditFlowConfig.ClientId, creditFlowConfig.ClientSecret, creditFlowConfig.Type));
            }
        }

        _financierRepository.Update(financier);
        await CreateAccounts(context.Message.Id, context.Message.TenantId);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task CreateAccounts(int financierId, int tenantId)
    {
        var financierAccounts = await _accountRepository.GetListAsync(financierId, tenantId);
        List<Account> accounts = [];

        if (!financierAccounts.Any(x => x.Type == AccountType.Commission))
        {
            var account = new Account(financierId, tenantId, AccountType.Commission, 0);
            accounts.Add(account);
        }

        if (!financierAccounts.Any(x => x.Type == AccountType.Bank))
        {
            var account = new Account(financierId, tenantId, AccountType.Bank, 0);
            accounts.Add(account);
        }

        if (accounts.Count > 0)
        {
            await _accountRepository.AddRangeAsync(accounts);
        }
    }
}
