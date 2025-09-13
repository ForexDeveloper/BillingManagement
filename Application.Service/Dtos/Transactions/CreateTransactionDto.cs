using Domain.Core.Entities.AccountAggregate;
using Domain.Core.Entities.TransactionAggregate;
using Domain.Core.Enums;

namespace Application.Service.Dtos.Transactions;

public class CreateTransactionDto
{
    public AccountType FromAccountType { get; set; }
    public int? FromBusinessIdentityId { get; set; }
    public Account FromAccount { get; set; }
    public int? FromAccountId { get; set; }
    public AccountType ToAccountType { get; set; }
    public int? ToBusinessIdentityId { get; set; }
    public Account ToAccount { get; set; }
    public int? ToAccountId { get; set; }
    public int TenantId { get; set; }
    public decimal Amount { get; set; }
    public TransactionType TransactionType { get; set; }
    public string TransactionDescription { get; set; }
    public long? ParentId { get; set; }
    public Transaction? Parent { get; set; }
}
