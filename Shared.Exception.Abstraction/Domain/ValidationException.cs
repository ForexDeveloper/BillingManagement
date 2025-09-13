using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Shared.Exception.Abstraction.Domain;

[Serializable]
public abstract class ValidationException : BaseException
{
    protected ValidationException()
    { }

    protected ValidationException(int code, string message) : base(code, message)
    { }

    protected ValidationException(string key, string message) : base(key, message) { }

    protected ValidationException(string message) : base(message)
    {
    }
    protected ValidationException(Collection<KeyValuePair<string, string>> detail) : base(detail)
    {
    }

    protected ValidationException(string message, System.Exception innerException) : base(message, innerException)
    {
    }
    protected ValidationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}