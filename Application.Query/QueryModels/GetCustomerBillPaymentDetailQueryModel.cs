using Domain.Core.Enums;
using Domain.Core.Helper;
using System;
using Application.Query.ViewModels.Customers;

namespace Application.Query.QueryModels
{
    public class GetCustomerBillPaymentDetailQueryModel
    {
        public decimal TotalAmount { get; set; }
        public DateTime PaymentDate { get; set; }
        public FinancialDocumentType PaymentType { get; set; }
        public PurchaseDetail PurchaseDetail { get; set; }
        public WalletChargeDetail WalletChargeDetail { get; set; }
        public BillingDetail BillingDetail { get; set; }
        public OperationalFeeDetail OperationalFeeDetail { get; set; }

        public GetCustomerBillPaymentDetailVm MapToViewModel()
        {
            var viewModel = new GetCustomerBillPaymentDetailVm
            {
                TotalAmount = this.TotalAmount,
                PaymentDate = this.PaymentDate,
                FinancialDocumentType = this.PaymentType,
                FinancialDocumentTypeName =Enum.GetName(this.PaymentType)
            };

            if (this.PurchaseDetail != null)
            {
                viewModel.PurchaseDetail = new ViewModels.Customers.PurchaseDetail()
                {
                    ReferenceNumber = this.PurchaseDetail.ReferenceNumber,
                    MerchantName = this.PurchaseDetail.MerchantName,
                    BranchName = this.PurchaseDetail.Branch,
                    BranchCode = this.PurchaseDetail.BranchCode,
                    CashAmount = this.PurchaseDetail.CashAmount,
                    CashWithdrawFrom = this.PurchaseDetail.CashWithdrawFrom,
                    CashTrackingCode = this.PurchaseDetail.CashTrackingCode,
                    CreditAmount = this.PurchaseDetail.CreditAmount,
                    CreditWithdrawFrom = this.PurchaseDetail.CreditWithdrawFrom,
                    CreditTrackingCode = this.PurchaseDetail.CreditTrackingCode
                };
            }

            if (this.WalletChargeDetail != null)
            {
                viewModel.WalletChargeDetail = new ViewModels.Customers.WalletChargeDetail()
                {
                    DepositTo = this.WalletChargeDetail.DepositTo,
                    TrackingCode = this.WalletChargeDetail.TrackingCode
                };
            }

            if (this.BillingDetail != null)
            {
                viewModel.BillingDetail = new ViewModels.Customers.BillingDetail()
                {
                    Wallet = this.BillingDetail.Wallet,
                    WithdrawFrom = this.BillingDetail.WithdrawFrom,
                    TrackingCode = this.BillingDetail.TrackingCode,
                    BillingDueDate = this.BillingDetail.BillingDueDate
                };
            }

            if (this.OperationalFeeDetail != null)
            {
                viewModel.OperationalFeeDetail = new ViewModels.Customers.OperationalFeeDetail()
                {
                    Wallet = this.OperationalFeeDetail.Wallet,
                    WithdrawFrom = this.OperationalFeeDetail.WithdrawFrom,
                    TrackingCode = this.OperationalFeeDetail.TrackingCode
                };
            }

            return viewModel;
        }
    }
    public class PurchaseDetail
    {
        public string ReferenceNumber { get; set; }
        public string MerchantName { get; set; }
        public string Branch { get; set; }
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