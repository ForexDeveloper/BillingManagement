using System.ComponentModel;

namespace Domain.Core.Enums
{
    public enum BmInterestReferenceType
    {
        [Description("مبلغ پیش پرداخت")]
        PrepaymentAmount = 1,

        [Description("مبلغ مازاد نقدی")]
        CashAmount = 2,

        [Description("مبلغ اعتباری")]
        CreditAmount = 3
    }
}
