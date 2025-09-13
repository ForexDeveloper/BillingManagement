using System.Globalization;

namespace Shared.Logging.Abstraction.Extensions;

public static class ExceptionExtensions
{
    private static IEnumerable<System.Exception> FromHierarchy(this Exception source, Func<Exception, Exception> nextItem,
        Func<System.Exception, bool> canContinue)
    {
        for (var current = source; canContinue(current); current = nextItem(current)) yield return current;
    }
    public static IDictionary<string, string> GetErrorsFromException(this Exception source, Func<Exception, Exception> nextItem)
    {
        if (null == nextItem)
            return new Dictionary<string, string>();

        return source.FromHierarchy(nextItem, s => s != null)
                     .Select((ex, index) => new { key = (++index).ToString(CultureInfo.CurrentCulture), value = ex.Message })
                     .ToDictionary(x => x.key, x => x.value);
    }
}