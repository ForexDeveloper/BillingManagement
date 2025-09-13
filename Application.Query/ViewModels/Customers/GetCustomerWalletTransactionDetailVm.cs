using Domain.Core.Enums;
using System;
using System.Text.Json.Serialization;

namespace Application.Query.ViewModels.Customers
{
    public class GetCustomerWalletTransactionDetailVm
    {
        public decimal Amount { get; set; }
        public DateTime Time { get; set; }
        public TransactionType Type { get; set; }
        public string TypeTitle { get; set; }
        public string Description { get; set; }
        public DateTime TransactionTime { get; internal set; }
        public string FromWalletName { get; set; }
        public string ToWalletName { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public PurchaseTransactionDetail PurchaseTransactionDetail { get; set; }


        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public WalletChargeTransactionDetail WalletChargeTransactionDetail { get; set; }


        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OperationalFeeTransactionDetail OperationalFeeTransactionDetail { get; set; }


        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public BaseTransactionDetail VerificationFeeTransactionDetail { get; set; }


        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ReverseTransactionDetail ReverseTransactionDetail { get; set; }


        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ReverseTransactionDetail RefundTransactionDetail { get; set; }


        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public BaseTransactionDetail WithdrawalTransactionDetail { get; set; }


        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public BaseTransactionDetail BillingTransactionDetail { get; set; }
    }

    public class PurchaseTransactionDetail : BaseTransactionDetail
    {
        public string TrackingCode { get; set; }
        public long FinancialDocumentId { get; set; }
    }

    public class ReverseTransactionDetail : BaseTransactionDetail
    {
        public long FinancialDocumentId { get; set; }
    }

    public class WalletChargeTransactionDetail : BaseTransactionDetail
    {
        public string TrackingCode { get; set; }
    }

    public class OperationalFeeTransactionDetail : BaseTransactionDetail
    {
        public string TrackingCode { get; set; }
        public OperationalFeeType OperationalFeeType { get; set; }
    }

    public class BaseTransactionDetail
    {
        public string WalletName { get; set; }
    }
}