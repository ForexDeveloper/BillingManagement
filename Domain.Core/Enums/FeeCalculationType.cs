using System.ComponentModel;

namespace Domain.Core.Enums
{
    public enum FeeCalculationType : byte
    {
        [Description("مبلغ ثابت سالانه به همراه کارمزد")]
        AnnualFixedAmountWithCommission = 1,

        [Description("مبلغ ثابت ماهانه به همراه کارمزد")]
        MonthlyFixedAmountWithCommission = 2,

        [Description("کارمزد بدون مبلغ ثابت")]
        CommissionWithoutFixedAmount = 3
    }
}
