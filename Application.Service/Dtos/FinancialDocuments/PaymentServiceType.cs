using System.ComponentModel;

namespace Application.Service.Dtos.FinancialDocuments;

public enum PaymentServiceType : byte
{
    [Description("خرید")]
    Purchase = 1,

    [Description("صورت حساب")]
    Billing = 2,

    [Description("هزینه عملیات")]
    OperationFee = 3,

    [Description("شارژ کیف پول")]
    CashIn = 4,

    [Description("هزینه اعتبارسنجی")]
    VerificationFee = 5
}
