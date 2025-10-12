using Domain.Core.Enums;
using System;

namespace Application.Service.Dtos.MerchantBillings;
public class MerchantBillingPayableDto
{
    public int TenantId { get; set; }
    public BillingStatus Status { get; set; }
    public DateTime DueDate { get; set; }
    public int GracePeriod { get; set; }
    public decimal PayableAmount { get; set; }
    public decimal PayAmount { get; set; }

    public MerchantBillingPayableDto(int tenantId, BillingStatus status, DateTime dueDate, int gracePeriod, decimal payableAmount, decimal payAmount)
    {
        TenantId = tenantId;
        Status = status;
        DueDate = dueDate;
        GracePeriod = gracePeriod;
        PayableAmount = payableAmount;
        PayAmount = payAmount;
    }
}
