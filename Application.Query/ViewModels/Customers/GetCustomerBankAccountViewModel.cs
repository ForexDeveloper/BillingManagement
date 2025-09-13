using Application.Query.QueryModels;
using Domain.Core.Enums;

namespace Application.Query.ViewModels.Customers;

public class GetCustomerBankAccountViewModel
{
    public GetCustomerBankAccountViewModel(GetCustomerBankAccountQueryModel bankAccount)
    {
        Id = bankAccount.Id;
        Iban = bankAccount.Iban;
        BankId = bankAccount.BankId;
        Status = bankAccount.Status;
        IsDefault = bankAccount.IsDefault;
    }
    public int Id { get; set; }
    public bool IsDefault { get;  set; }
    public string Iban { get;  set; }
    public int? BankId { get;  set; }
    public BankAccountStatus Status { get;  set; }
}