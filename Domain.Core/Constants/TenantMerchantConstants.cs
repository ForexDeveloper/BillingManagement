namespace Domain.Core.Constants;

public static class TenantMerchantConstants
{
    public static string FixedAmountCommissionDescription => ".جمع تعداد تراکنش های دوره ضربدر مبلغ ثابت می شود";

    public static string FixedPercentageCommissionDescription => ".جمع مبلغ تراکنش های دوره ضربدر درصد کارمزد می شود";

    public static string CumulativeTieredCommissionDescription => ".جمع تراکنش های خرید نهایی شده در دوره صورتحساب محاسبه و درصد هر بخش از مبلغ در پلکان خودش محاسبه می شود";

    public static string UniformedTieredCommissionDescription => ".جمع تراکنش های خرید نهایی شده در دوره صورتحساب محاسبه و اگر کل مبلغ در یک بازه قرار بگیرد، درصد همان بازه به کل مبلغ اعمال می شود";
}