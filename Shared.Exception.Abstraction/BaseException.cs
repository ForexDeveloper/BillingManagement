using System.Collections.ObjectModel;
using System.Globalization;
using System.Runtime.Serialization;

namespace Shared.Exception.Abstraction;

public abstract class BaseException : System.Exception
{
    public Collection<KeyValuePair<string, string>> Details { get; } = new();

    protected BaseException(int code, string message)
    {
        Details.Add(new KeyValuePair<string, string>(code.ToString(CultureInfo.InvariantCulture), message));
    }

    protected BaseException(string key, string message)
    {
        Details.Add(new KeyValuePair<string, string>(key, message));
    }

    protected BaseException(Collection<KeyValuePair<string, string>> detail)
    {
        Details = detail;
    }

    protected BaseException()
    {
    }

    protected BaseException(string message) : base(message)
    {
        Details.Add(new KeyValuePair<string, string>("0", message));
    }

    protected BaseException(string message, System.Exception innerException) : base(message, innerException)
    {
    }

    protected BaseException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}