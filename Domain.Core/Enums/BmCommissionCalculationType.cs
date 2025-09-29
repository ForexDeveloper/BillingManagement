using System.ComponentModel;

namespace Domain.Core.Enums
{
    public enum BmCommissionCalculationType
    {
        [Description("کارمزد پلکانی یکنواخت")]
        UniformTiered = 1,

        [Description("کارمزد پلکانی تجمعی")]
        CumulativeTiered = 2,

        [Description("کارمزد درصد ثابت")]
        FixedPercentage = 3,

        [Description("کارمزد مبلغ ثابت")]
        FixedAmount = 4
    }
}
