namespace Service.Rest.V1.RequestModels.Billings;

public class CreateCustomerBankAccountModel
{
    public string Iban { get; set; }
    public string ShamsiBirthDate { get; set; }
}