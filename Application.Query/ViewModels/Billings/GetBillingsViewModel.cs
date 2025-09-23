using System;
using Domain.Core.Enums;
using Application.Query.Base;

namespace Application.Query.ViewModels.Billings;

public sealed class GetBillingsViewModel : BasePaginatedListQueryResult<GetBillingsItemViewModel>;

public sealed class GetBillingsItemViewModel
{
    public long Id { get; set; }

    public string Code { get; set; }

    public BillingType Type { get; set; }

    public string TypeTitle { get; set; }

    public BillingStatus Status { get; set; }

    public string StatusTitle { get; set; }

    public DateTime DueDate { get; set; }

    public DateTime EndDate { get; set; }

    public decimal PayableAmount { get; set; }
}