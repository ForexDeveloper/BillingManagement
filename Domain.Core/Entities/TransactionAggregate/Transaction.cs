using Domain.Base;
using Domain.Core.Entities.AccountAggregate;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Enums;
using Domain.Core.Helper;
using System.Collections.Generic;

namespace Domain.Core.Entities.TransactionAggregate;

public class Transaction : BaseEntity<long>
{
    #region Property

    public int FromAccountId { get; private set; }
    public Account FromAccount { get; private set; }

    public int ToAccountId { get; private set; }
    public Account ToAccount { get; private set; }
    public int TenantId { get; private set; }
    public Tenant Tenant { get; private set; }
    public decimal Amount { get; private set; }

    public TransactionType Type { get; private set; }

    public long? ParentId { get; private set; }
    public Transaction Parent { get; private set; }
    public ICollection<Transaction> ChildTransactions { get; private set; }
    public string Description { get; private set; }
    public string CheckSum { get; private set; }

    #endregion Property

    private Transaction() { }

    public Transaction(int fromAccountId, int toAccountId, int tenantId, decimal amount, TransactionType type,
        string description)
    {
        FromAccountId = fromAccountId;
        ToAccountId = toAccountId;
        TenantId = tenantId;
        Type = type;
        Description = description;
        SetAmount(amount);
        SetCheckSum();
    }

    public Transaction(Account fromAccount, Account toAccount, int tenantId, decimal amount, TransactionType type,
        string description)
    {
        FromAccount = fromAccount;
        FromAccountId = fromAccount.Id;
        ToAccount = toAccount;
        ToAccountId = ToAccount.Id;
        TenantId = tenantId;
        Type = type;
        Description = description;
        SetAmount(amount);
        SetCheckSum();
    }

    public void SetAmount(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentValidationException(nameof(amount), "مبلغ تراکنش نمی تواند کوچک تر مساوی صفر باشد");

        Amount = amount;
    }

    public void SetCheckSum()
    {
        CheckSum = $"{FromAccountId}{ToAccountId}{Amount:F10}{CreatedDateTime:yyyy-MM-ddTHH:mm:ss}".Hash();
    }

    public void SetParentId(long? parentId)
    {
        ParentId = parentId;
    }

    public void SetParent(Transaction parent)
    {
        Parent = parent;
    }

    public void SetBalance(decimal amount)
    {
        ValidateCheckSum();

        Amount += amount;
        SetCheckSum();
    }

    public void ValidateCheckSum()
    {
        string comperedTo = $"{FromAccountId}{ToAccountId}{Amount:F10}{CreatedDateTime:yyyy-MM-ddTHH:mm:ss}";

        if (!comperedTo.Validate(CheckSum))
            throw new ArgumentValidationException("InconsistentData", "اطلاعات موجود در دیتابیس صحیح نمی باشد.");
    }
}
