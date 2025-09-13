using System;

namespace Application.Service.Helper;

public static class RoundHelper
{
    public static decimal FloorAmount(decimal amount)
    {
        decimal roundedAmount = Math.Floor(amount / 1000m) * 1000m;

        return roundedAmount;
    }

    public static decimal CeilingAmount(decimal amount)
    {
        decimal roundedAmount = Math.Ceiling(amount / 1000m) * 1000m;

        return roundedAmount;
    }

    public static decimal RoundAmount(decimal amount)
    {
        return Math.Round(amount, 0, MidpointRounding.AwayFromZero);
    }

    public static decimal TruncateAmount(decimal amount)
    {
        return Math.Truncate(amount);
    }
}
