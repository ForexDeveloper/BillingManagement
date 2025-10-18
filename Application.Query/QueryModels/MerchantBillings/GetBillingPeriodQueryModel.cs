using System;
using System.Collections.Generic;

namespace Application.Query.QueryModels.MerchantBillings;

public sealed record GetBillingPeriodQueryModel
{
    public DateTime StartDate { get; set; }

    public DateTime DueDate { get; set; }

    public List<int> ContractIds { get; set; }
}