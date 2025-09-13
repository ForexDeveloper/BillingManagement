namespace Service.Rest.V1.RequestModels.Transactions;

public class TestGrantingModel
{
    public int TenantId { get; set; }
    public int CustomerId { get; set; }
    public int UserCreditGrantingProcessId { get; set; }
    public int CreditGrantingProcessId { get; set; }
    public int PlanId { get; set; }
    public decimal InitialCreditAmount { get; set; }
    public decimal OperationalFeeAmount { get; set; }
    public decimal VerificationFeeAmount { get; set; }
    public decimal OperationalFee { get; set; }
    public byte OperationalFeeType { get; set; }
    public int NumberOfInstallment { get; set; }
}
