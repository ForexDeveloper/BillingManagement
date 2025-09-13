using Domain.Core.Enums;
using System.Collections.Generic;

namespace Application.Service.Dtos.Transactions;

public class TransactionDto
{
    public int FromAccountId { get; set; }
    public int ToAccountId { get; set; }
    public int TenantId { get; set; }
    public decimal Amount { get; set; }
    public TransactionType TransactionType { get; set; }
    public string Description { get; set; }
    public long? ParentId { get; set; }
    public List<TransactionDto> Children { get; set; } = new List<TransactionDto>();
}
