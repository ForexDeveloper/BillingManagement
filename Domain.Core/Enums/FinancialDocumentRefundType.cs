using System.ComponentModel;

namespace Domain.Core.Enums;

public enum FinancialDocumentRefundType : byte
{
    [Description("عودت کامل")]
    CompleteRefund = 1,

    [Description("عودت جزئی")]
    PartiallyRefund = 2,
}
