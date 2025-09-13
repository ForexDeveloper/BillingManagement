using System.ComponentModel;

namespace Domain.Core.Enums;

public enum FinancialDocumentPaymentType : byte
{
    [Description("اعتباری")]
    Credit = 1,

    [Description("نقدی")]
    Cash = 2,

    [Description("پیش پرداخت")]
    Prepayment = 3,

    [Description("چک")]
    Cheque = 4
}
