using Application.Query.Base;
using Domain.Core.Enums;
using Shared.Utilities.Extensions;
using System;

namespace Application.Query.ViewModels.Tenants;

public class GetPurchasesListVm : BasePaginatedListQueryResult<PurchaseVm>
{

}
public class GetPurchaseVm : PurchaseVm
{
    public string CustomerFullName { get; set; }
    public string CustomerNationalCode { get; set; }
    public string CustomerMobileNumber { get; set; }
    public string BranchName { get; set; }
    public string BranchCode { get; set; }
}

public class PurchaseVm
{
    public long Id { get; set; }
    public int WalletId { get; set; }

    public string WalletName { get; set; }

    public string OrganizationName { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal CashAmount { get; set; }

    public decimal CreditAmount { get; set; }

    public string MerchantName { get; set; }

    public string ReferenceNumber { get; set; }

    public DateTime CreateDateTime { get; set; }

    public FinancialDocumentState State { get; set; }
    public string StateTitle => State.GetEnumDescription();

    public PaymentGatewayType? PaymentGatewayType { get; set; }
    public string? PaymentGatewayTypeTitle => PaymentGatewayType?.GetEnumDescription();
}