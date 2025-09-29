using System.ComponentModel;

namespace Domain.Core.Enums
{
    public enum BmCommissionReferenceType
    {
        [Description("مبلغ پیش پرداخت")]
        PrepaymentAmount = 1,

        [Description("مبلغ مازاد نقدی")]
        CashAmount = 2,

        [Description("مبلغ اعتباری")]
        CreditAmount = 3,

        [Description("مبلغ بهره")]
        InterestAmount = 4
    }
}
