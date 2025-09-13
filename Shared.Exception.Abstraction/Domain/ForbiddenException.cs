using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Shared.Exception.Abstraction.Domain;

[Serializable]
public abstract class ForbiddenException : BaseException
{
    protected ForbiddenException()
    { }

    protected ForbiddenException(int code, string message) : base(code, message)
    { }

    protected ForbiddenException(string message) : base(message)
    {
    }

    protected ForbiddenException(Collection<KeyValuePair<string, string>> detail) : base(detail)
    {
    }

    protected ForbiddenException(string message, System.Exception innerException) : base(message, innerException)
    {
    }
    protected ForbiddenException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}