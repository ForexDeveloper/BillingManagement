using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Shared.Exception.Abstraction.Domain;

[Serializable]
public abstract class DuplicateException : BaseException
{
    protected DuplicateException()
    { }

    protected DuplicateException(int code, string message) : base(code, message) { }

    protected DuplicateException(string message) : base(message)
    {
    }

    protected DuplicateException(Collection<KeyValuePair<string, string>> detail) : base(detail)
    {
    }

    protected DuplicateException(string message, System.Exception innerException) : base(message, innerException)
    {
    }
    protected DuplicateException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}