using Application.Service.Contracts;
using Application.Service.Dtos.Transactions;
using Domain.Core.Entities.AccountAggregate;
using Domain.Core.Entities.AccountAggregate.Exceptions;
using Domain.Core.Entities.TransactionAggregate;
using Domain.Core.Enums;
using Domain.Core.Helper;
using Domain.Core.UnitOfWorkContracts;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace Application.Service.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IApplicationDbContextUnitOfWork _unitOfWork;

    public TransactionService(ITransactionRepository transactionRepository, IAccountRepository accountRepository, IApplicationDbContextUnitOfWork unitOfWork)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<Domain.Core.Entities.TransactionAggregate.Transaction>> 
        CreateTransactionAsync(List<TransactionDto> newTransactions, TransactionScope transactionScope)
    {
        var accountIds = GetAllAccountIds(newTransactions);

        var accounts = await _accountRepository.GetByIdsWithPessimisticLockAsync(accountIds);

        if (accounts.Count != accountIds.Count || accountIds.Count == 0)
        {
            throw new AccountNotFoundException("حساب مورد نظر یافت نشد");
        }

        List<Domain.Core.Entities.TransactionAggregate.Transaction> transactions = [];

        foreach (var newTransaction in newTransactions)
        {
            AddTransactionRecursive(transactions, accounts, newTransaction);
        }

        await _transactionRepository.AddRangeAsync(transactions);
        await _unitOfWork.SaveChangesAsync();
        return transactions;
    }

    public void AddTransactionRecursive(List<Domain.Core.Entities.TransactionAggregate.Transaction> transactions, 
        List<Account> accounts, TransactionDto newTransaction,
        Domain.Core.Entities.TransactionAggregate.Transaction? parentTransaction = null)
    {
        var transaction = CreateTransaction(accounts, newTransaction, parentTransaction);

        transactions.Add(transaction);

        if (newTransaction.Children != null)
        {
            foreach (var child in newTransaction.Children)
            {
                AddTransactionRecursive(transactions, accounts, child, transaction);
            }
        }
    }


    private Domain.Core.Entities.TransactionAggregate.Transaction CreateTransaction(
        List<Account> accounts, TransactionDto newTransaction, 
        Domain.Core.Entities.TransactionAggregate.Transaction parentTransaction = null)
    {
        var fromAccount = accounts.FirstOrDefault(x => x.Id == newTransaction.FromAccountId);

        var toAccount = accounts.FirstOrDefault(x => x.Id == newTransaction.ToAccountId);

        if (fromAccount.Status != AccountStatus.Active)
        {
            throw new AccountNotFoundException($"حساب {fromAccount.Type.GetEnumDescription()} غیرفعال می باشد.");
        }

        if (toAccount.Status != AccountStatus.Active)
        {
            throw new AccountNotFoundException($"حساب {toAccount.Type.GetEnumDescription()} غیرفعال می باشد.");
        }

        var transaction = new Domain.Core.Entities.TransactionAggregate.Transaction(
            fromAccount.Id,
            toAccount.Id,
            newTransaction.TenantId,
            newTransaction.Amount,
            newTransaction.TransactionType,
            newTransaction.Description
        );

        if (parentTransaction != null)
        {
            transaction.SetParent(parentTransaction);
        }
        else if (newTransaction.ParentId != null && newTransaction.ParentId > 0)
        {
            transaction.SetParentId(newTransaction.ParentId);
        }

        fromAccount.DecreaseBalance(newTransaction.Amount);
        toAccount.IncreaseBalance(newTransaction.Amount);
        
        return transaction;
    }

    //TODO contracts update
    public async Task<bool> HasTransactionByContractNumber(string contractNumber)
    {
        return false;
    }

    private List<int> GetAllAccountIds(List<TransactionDto> transactions)
    {
        var allTransactions = FlattenTransactions(transactions);

        var accountIds = allTransactions
            .SelectMany(t => new[] { t.FromAccountId, t.ToAccountId })
            .Distinct()
            .ToList();

        return accountIds;
    }

    private List<TransactionDto> FlattenTransactions(List<TransactionDto> transactions)
    {
        var result = new List<TransactionDto>();

        foreach (var tx in transactions)
        {
            result.Add(tx);

            if (tx.Children != null && tx.Children.Any())
            {
                result.AddRange(FlattenTransactions(tx.Children));
            }
        }

        return result;
    }
}
