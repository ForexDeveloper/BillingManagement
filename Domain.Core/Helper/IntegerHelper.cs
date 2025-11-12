namespace Domain.Core.Helper;

public static class IntegerHelper
{
    public static string GetLast2Digits(this int value)
    {
        var last2Digits = (value % 100).ToString().PadLeft(2, '0');

        return last2Digits;
    }
}