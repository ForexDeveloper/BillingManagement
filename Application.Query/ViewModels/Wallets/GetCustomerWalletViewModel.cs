using Domain.Core.Enums;
using Domain.Core.Helper;
using System;

namespace Application.Query.ViewModels.Wallets;

public class GetCustomerWalletViewModel
{
    public decimal Balance { get; set; }
    public WalletType Type { get; set; }
    public string Title { get; set; }
    public decimal? LastBillAmount { get; set; }
    public DateTime? LastBillDate { get; set; }
    public BillingState? BillStatus { get; set; }
    public long? LastBillId { get; set; }
    public string LogoId { get; set; }
    public WalletStatus Status { get; set; }
    public string BillStatusTitle => BillStatus.GetEnumDescription();
    public string StatusTitle => Status.GetEnumDescription();
    public string TypeTitle => Type.GetEnumDescription();
    public bool IsDefault { get; set; }
    public string TermsAndConditions { get; set; }
}