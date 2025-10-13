using Domain.Core.Enums;
using System;

namespace Application.Query.QueryModels.MerchantBillings;

public class GetMerchantBillingQueryModel
{
    public long Id { get; set; }
    public int GracePeriod { get; set; }
    public BillingStatus Status { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal PayableAmount { get; set; }
}
