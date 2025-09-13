using System.ComponentModel;

namespace Domain.Core.Enums
{
    public enum OperationalFeeType : byte
    {
        [Description("کسر هزینه اشتراک از مبلغ تسهیلات")]
        DeductFromLoan = 1,
        [Description("اضافه شدن هزینه اشتراک به تسهیلات و تقسیط آن")]
        AddToLoanInstallment = 2,
        [Description("پرداخت آنلاین هزینه اشتراک")]
        OnlinePayment = 3,
    }
}
