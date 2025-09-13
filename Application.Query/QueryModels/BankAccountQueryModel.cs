using Domain.Core.Entities.BankAccountAggregate;
using Domain.Core.Enums;

namespace Application.Query.QueryModels;

public class GetCustomerBankAccountQueryModel
{
    public GetCustomerBankAccountQueryModel(BankAccount bankAccount)
    {
        Id = bankAccount.Id;
        Iban = bankAccount.Iban;
        BankId = bankAccount.BankId;
        Status = bankAccount.Status;
        IsDefault = bankAccount.IsDefault;
    }

    public int Id { get; set; }
    public bool IsDefault { get; set; }
    public string Iban { get; set; }
    public int? BankId { get; set; }
    public BankAccountStatus Status { get; set; }
}