using Application.Service.Dtos.Transactions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;

namespace Application.Service.Contracts;

public interface ITransactionService
{
    Task<List<Domain.Core.Entities.TransactionAggregate.Transaction>> CreateTransactionAsync(List<TransactionDto> transactions, TransactionScope transactionScope);
    Task<bool> HasTransactionByContractNumber(string contractNumber);
}
