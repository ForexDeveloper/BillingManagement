using System.ComponentModel;

namespace Domain.Core.Enums;

public enum FinancialDocumentType : byte
{
    [Description("خرید")]
    Purchase = 1,

    [Description("عودت وجه")]
    Refund = 2,

    [Description("برگشت خرید")]
    Reverse = 3
}
