using Domain.Core.Enums;
using Domain.Core.Helper;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Application.Query.QueryModels
{
    public class CustomerFinancialDocumentDetailQueryModel
    {
        public FinancialDocumentType Type { get; set; }
        public string TypeTitle => Type.GetEnumDescription();
        public decimal Amount { get; set; }
        public DateTime Time { get; set; }
        public string Description { get; set; }
        public FinancialDocumentState State { get; set; }
        public string StateTitle => State.GetEnumDescription();
        public PaymentGatewayType? PaymentGatewayType { get; set; }
        public string? PaymentGatewayTypeTitle => PaymentGatewayType.GetEnumDescription();

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public PurchaseDocumentDetail PurchaseDetail { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public WalletChargeDocumentDetail WalletChargeDetail { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public BillingDocumentDetail BillingDetail { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OperationalFeeDocumentDetail OperationalFeeDetail { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public BaseFinancialDocumentDetail VerificationFeeDetail { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public RefundDocumentDetail RefundDetail { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public WithdrawCashOutDocumentDetail WithdrawCashOutDetail { get; set; }
    }

    public class PurchaseDocumentDetail
    {
        public string ReferenceIdentity { get; set; }
        public string MerchantName { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public PaymentDetail CashDeposit { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public PaymentDetail CreditDeposit { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<RefundDocumentDetail>? RefundDetails { get; set; } = null;
    }

    public class RefundDocumentDetail
    {
        public long? ParentId { get; set; }
        public string MerchantName { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public PaymentDetail CashDeposit { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public PaymentDetail CreditDeposit { get; set; }
    }

    public class WithdrawCashOutDocumentDetail
    {
        public long? ParentId { get; set; }
        public PaymentDetail CashDeposit { get; set; }
    }

    public class WalletChargeDocumentDetail : BaseFinancialDocumentDetail
    {
        public string TrackingCode { get; set; }
    }

    public class BillingDocumentDetail : BaseFinancialDocumentDetail
    {
        public DateTime BillingDate { get; set; }
        public string BillingWalletName { get; set; }
        public string TrackingCode { get; set; }
        public WalletSettlementType SettlementType { get; set; }
    }


    public class OperationalFeeDocumentDetail : BaseFinancialDocumentDetail
    {
        public OperationalFeeType OperationalFeeType { get; set; }
        public string TrackingCode { get; set; }
    }

    public class PaymentDetail : BaseFinancialDocumentDetail
    {
        public decimal Amount { get; set; }
        public string TrackingCode { get; set; }
    }

    public class BaseFinancialDocumentDetail
    {
        public string WalletName { get; set; }
    }
}
