using Domain.Core.Enums;
using Domain.Core.Helper;
using System;

namespace Application.Query.QueryModels;

public class GetCustomerWalletTransactionDetailQueryModel
{
    public long TransactionId { get; set; }
    public decimal Amount { get; set; }
    public DateTime TransactionTime { get; set; }
    public TransactionType Type { get; set; }
    public string TypeTitle => Type.GetEnumDescription();

    public string Description { get; set; }

    public string FromWalletName { get; set; }
    public string ToWalletName { get; set; }

    public PurchaseTransactionDetail PurchaseTransactionDetail { get; set; }

    public WalletChargeTransactionDetail WalletChargeTransactionDetail { get; set; }

    public OperationalFeeTransactionDetail OperationalFeeTransactionDetail { get; set; }

    public BaseTransactionDetail VerificationFeeTransactionDetail { get; set; }

    public BaseTransactionDetail BillingTransactionDetail { get; set; }

    public ReverseTransactionDetail ReverseTransactionDetail { get; set; }

    public ReverseTransactionDetail RefundTransactionDetail { get; set; }

    public BaseTransactionDetail WithdrawalTransactionDetail { get; set; }


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
