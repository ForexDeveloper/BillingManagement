using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Shared.Exception.Abstraction.Domain;

[Serializable]
public abstract class UnauthorizedException : BaseException
{
    protected UnauthorizedException()
    { }

    protected UnauthorizedException(int code, string message) : base(code, message)
    { }

    protected UnauthorizedException(string message) : base(message)
    {
    }

    protected UnauthorizedException(Collection<KeyValuePair<string, string>> detail) : base(detail)
    {
    }

    protected UnauthorizedException(string message, System.Exception innerException) : base(message, innerException)
    {
    }
    protected UnauthorizedException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}