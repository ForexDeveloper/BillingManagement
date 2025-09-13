using System.Globalization;

namespace Shared.Middlewares.Extensions;

public static class ExceptionExtensions
{
    private static IEnumerable<System.Exception> FromHierarchy(this System.Exception source, Func<System.Exception, System.Exception> nextItem,
        Func<System.Exception, bool> canContinue)
    {
        for (var current = source; canContinue(current); current = nextItem(current)) yield return current;
    }
    public static IDictionary<string, string> GetErrorsFromHierarchy(this System.Exception source, Func<System.Exception, System.Exception> nextItem)
    {
        return source.FromHierarchy(nextItem, s => s != null)
                     .Select((ex, index) => new { key = (++index).ToString(CultureInfo.InvariantCulture), value = ex.Message })
                     .ToDictionary(x => x.key, x => x.value);
    }
}