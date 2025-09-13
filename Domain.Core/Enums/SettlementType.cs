using System.ComponentModel;

namespace Domain.Core.Enums
{
    public enum SettlementType : byte
    {
        [Description("در تاریخ سررسید، کل مبلغ یکجا پرداخت می‌شود")]
        LumpSum = 1,

        [Description("در تاریخ سررسید، کل مبلغ به صورت اقساط پرداخت می‌شود")]
        Installments = 2
    }
}
