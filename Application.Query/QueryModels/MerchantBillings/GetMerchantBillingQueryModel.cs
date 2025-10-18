using System;
using Domain.Core.Enums;

namespace Application.Query.QueryModels.MerchantBillings;

public class GetMerchantBillingQueryModel
{
    public long Id { get; set; }

    public int GracePeriod { get; set; }

    public BillingStatus Status { get; set; }

    public DateTime DueDate { get; set; }

    public DateTime PaymentDeadlineDate { get; set; }

    public decimal PayableAmount { get; set; }
}