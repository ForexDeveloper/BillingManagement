using Domain.Core.Enums;
using Domain.Core.Helper;
using System;
using System.Text.Json.Serialization;

namespace Application.Query.ViewModels.Customers
{

    public class GetCustomerBillPaymentDetailVm
    {
        public decimal TotalAmount { get; set; }
        public DateTime PaymentDate { get; set; }
        public FinancialDocumentType FinancialDocumentType { get; set; }

        public string FinancialDocumentTypeName
        {
            get => FinancialDocumentType.GetEnumDescription();
            set=> Enum.Parse<FinancialDocumentType>(value);
           
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public PurchaseDetail PurchaseDetail { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public WalletChargeDetail WalletChargeDetail { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public BillingDetail BillingDetail { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OperationalFeeDetail OperationalFeeDetail { get; set; }
    }
    public class PurchaseDetail
    {
        public string ReferenceNumber { get; set; }
        public string MerchantName { get; set; }
        public string BranchName { get; set; }
        public int? BranchCode { get; set; }

        public decimal? CashAmount { get; set; }
        public string CashWithdrawFrom { get; set; }
        public int? CashTrackingCode { get; set; }

        public decimal? CreditAmount { get; set; }
        public string CreditWithdrawFrom { get; set; }
        public int? CreditTrackingCode { get; set; }
    }

    public class WalletChargeDetail
    {
        public string DepositTo { get; set; }
        public int? TrackingCode { get; set; }
    }

    public class BillingDetail
    {
        public string Wallet { get; set; }
        public string WithdrawFrom { get; set; }
        public int? TrackingCode { get; set; }
        public DateTime? BillingDueDate { get; set; }
    }
    public class OperationalFeeDetail
    {
        public string Wallet { get; set; }
        public string WithdrawFrom { get; set; }
        public int? TrackingCode { get; set; }
    }   

}