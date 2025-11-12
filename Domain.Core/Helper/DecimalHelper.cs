namespace Domain.Core.Helper;

public static class DecimalHelper
{
    public static decimal Normalize(this decimal value)
    {
        return value / 1.000000000000000000000000000000000m;
    }

    public static string CommaSeparate(this decimal value)
    {
        return value.ToString("#,##0.##");
    }
}