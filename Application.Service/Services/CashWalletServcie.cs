using Application.Service.Contracts;
using Application.Service.Dtos.BillingPayments;
using Application.Service.Dtos.CashWallet;
using Domain.Core.Entities.AccountAggregate;
using Domain.Core.Entities.WalletAggregate;
using Domain.Core.Entities.WalletContractAggregate;
using Domain.Core.Enums;
using System;
using System.Threading.Tasks;

namespace Application.Service.Services;

public class CashWalletService : ICashWalletService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IWalletContractRepository _walletContractRepository;
    private readonly IWalletRepository _walletRepository;

    public CashWalletService(
        IAccountRepository accountRepository,
        IWalletContractRepository walletContractRepository,
        IWalletRepository walletRepository)
    {
        _accountRepository = accountRepository;
        _walletContractRepository = walletContractRepository;
        _walletRepository = walletRepository;
    }

    public async Task CreateCustomerCashWalletByAddCustomerEvent(CreateCashWalletWithAddCustomerEventDto dto)
    {
        var customerHasWallet = await _walletRepository.HasCashWalletAsync(dto.CustomerId, dto.TenantId);
        if (customerHasWallet)
            return;

        var walletContractPlan = await
            _walletContractRepository.GetWalletContractPlanForCashWallet(dto.TenantId);

        if (walletContractPlan == null)
            throw new Exception("قرارداد کیف پول یا پلن یافت نشد");

        var customerCashWallet = new CreateNewCustomerCashWalletDto()
        {
            PlanId = walletContractPlan.PlanId,
            CustomerId = dto.CustomerId,
            TenantId = dto.TenantId,
            WalletContractId = walletContractPlan.WalletContractId,
        };

        await CreateCustomerCashWallet(customerCashWallet);
    }

    private async Task CreateCustomerCashWallet(CreateNewCustomerCashWalletDto customerWallet)
    {
        var customerWalletAccount = await _accountRepository.GetAsync(AccountType.CashWallet, customerWallet.CustomerId);
        if (customerWalletAccount == null)
        {
            customerWalletAccount = new Account(customerWallet.CustomerId, customerWallet.TenantId, AccountType.CashWallet, 0);
            await _walletRepository.AddAsync(new CashWallet(customerWallet.CustomerId, customerWallet.TenantId, customerWalletAccount, customerWallet.PlanId, customerWallet.WalletContractId));
        }
        else
        {
            await _walletRepository.AddAsync(new CashWallet(customerWallet.CustomerId, customerWallet.TenantId, customerWalletAccount.Id, customerWallet.PlanId, customerWallet.WalletContractId));
        }

    }

}
