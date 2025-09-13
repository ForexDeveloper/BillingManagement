using Application.Service.Contracts;
using Application.Service.Dtos.CashWallet;
using Domain.Core.Entities.AccountAggregate;
using Domain.Core.Entities.CustomerAggregate;
using Domain.Core.Entities.WalletAggregate;
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

public class CustomerAddedOrUpdatedEventConsumer : IConsumer<CmCustomerAddedOrUpdatedEvent>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly ICashWalletService _iCashWalletService;
    private readonly IApplicationDbContextUnitOfWork _unitOfWork;
    public readonly ILogger<CustomerAddedOrUpdatedEventConsumer> _logger;

    public CustomerAddedOrUpdatedEventConsumer(ILogger<CustomerAddedOrUpdatedEventConsumer> logger, ICustomerRepository customerRepository,
         IAccountRepository accountRepository, IApplicationDbContextUnitOfWork unitOfWork, ICashWalletService cashWalletService, IWalletRepository walletRepository)
    {
        _logger = logger;
        _customerRepository = customerRepository;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _iCashWalletService = cashWalletService;
        _walletRepository = walletRepository;
    }

    public async Task Consume(ConsumeContext<CmCustomerAddedOrUpdatedEvent> context)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        var succeed = true;
        try
        {
            var existedCustomers = await _customerRepository.GetCustomersByIds(context.Message.Customers.Select(x => x.Id).ToList(), context.Message.TenantId);
            foreach (var customer in context.Message.Customers)
            {
                var existCustomer = existedCustomers.FirstOrDefault(p => p.Id == customer.Id);
                if (existCustomer == null)
                {
                    var newCustomer = new Customer(customer.Id, customer.FullName, customer.Mobile, customer.NationalId, context.Message.TenantId);
                    if (context.Message.OrganizationId.HasValue)
                        newCustomer.SetCustomerOrganizations(context.Message.OrganizationId.Value);

                    await _customerRepository.AddAsync(newCustomer);
                    await _unitOfWork.SaveChangesAsync();
                    await CreateCustomerAccounts(newCustomer.Id, newCustomer.TenantId);
                    //await _unitOfWork.SaveChangesAsync();

                    if (customer.IsConfirmShahkar)
                        await CreateCashWallet(context.Message.TenantId, newCustomer.Id);
                }
                else
                {
                    existCustomer.Update(customer.FullName, customer.Mobile);

                    if (context.Message.OrganizationId.HasValue && !existCustomer.CustomerOrganizations
                        .Any(x => x.OrganizationId == context.Message.OrganizationId))
                    {
                        existCustomer.SetCustomerOrganizations(context.Message.OrganizationId.Value);
                    }
                    _customerRepository.Update(existCustomer);
                    await CreateCustomerAccounts(existCustomer.Id, existCustomer.TenantId);
                    //await _unitOfWork.SaveChangesAsync();

                    if (customer.IsConfirmShahkar)
                        await CreateCashWallet(context.Message.TenantId, existCustomer.Id);
                }
            }
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogCritical(new LogStruct
            {
                Message = ex.Message,
                ServiceName = "CustomerAddedOrUpdatedEventConsumer_Consume",
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
                ServiceName = "CustomerAddedOrUpdatedEventConsumer_Consume",
                InputParams = context.Message,
                Results = succeed,
                ResponseTimeStopWatcher = stopWatch,
                Tags = LogMessageTag.EventBus,
            });
        }
    }

    private async Task CreateCashWallet(int tenantId, int customerId)
    {
        await _iCashWalletService.CreateCustomerCashWalletByAddCustomerEvent(new CreateCashWalletWithAddCustomerEventDto()
        {
            TenantId = tenantId,
            CustomerId = customerId
        });
    }

    private async Task CreateCustomerAccounts(int customerId, int tenantId)
    {
        var customerAccounts = await _accountRepository.GetListAsync(customerId, tenantId);
        List<Account> accounts = [];

        //if (!customerAccounts.Any(x => x.Type == AccountType.Loan))
        //{
        //    var customerLoanAccount = new Account(customerId, tenantId, AccountType.Loan, 0);
        //    accounts.Add(customerLoanAccount);
        //}

        if (!customerAccounts.Any(x => x.Type == AccountType.Bank))
        {
            var customerBankAccount = new Account(customerId, tenantId, AccountType.Bank, 0);
            accounts.Add(customerBankAccount);
        }

        if (accounts.Count != 0)
        {
            await _accountRepository.AddRangeAsync(accounts);
        }
    }
}
