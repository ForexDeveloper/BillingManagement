using System.ComponentModel;

namespace Domain.Core.Enums
{
    public enum CommissionType : byte
    {
        [Description("تعداد")]
        TransactionCount = 1,
        [Description("مبلغ تراکنش")]
        TransactionAmount = 2
    }
}
