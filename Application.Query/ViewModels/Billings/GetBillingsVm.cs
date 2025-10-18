using System;
using Domain.Core.Enums;
using Application.Query.Base;

namespace Application.Query.ViewModels.Billings;

public sealed class GetBillingsVm : BasePaginatedListQueryResult<GetBillingsItemVm>;

public sealed class GetBillingsItemVm
{
    public long Id { get; set; }

    public string Code { get; set; }

    public string MerchantTitle { get; set; }

    public BillingType Type { get; set; }

    public string TypeTitle { get; set; }

    public BillingStatus Status { get; set; }

    public string StatusTitle { get; set; }

    public DateTime DueDate { get; set; }

    public DateTime PaymentDeadlineDate { get; set; }

    public decimal PayableAmount { get; set; }
}