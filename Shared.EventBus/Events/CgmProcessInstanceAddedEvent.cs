using Shared.EventBus.Contracts;

namespace Shared.EventBus.Events;

public class CgmProcessInstanceAddedEvent : IEventSign
{
    public int TenantId { get; set; }
    public int CustomerId { get; set; }
    //public string UserId { get; set; }
    public int UserCreditGrantingProcessId { get; set; }
    public int CreditGrantingProcessId { get; set; }

    public int PlanId { get; set; }
    public decimal InitialCreditAmount { get; set; }

    public decimal OperationalFee { get; set; }
    public decimal OperationalFeeAmount { get; set; }
    public byte OperationalFeeType { get; set; }
    public int NumberOfInstallment { get; set; }
}
