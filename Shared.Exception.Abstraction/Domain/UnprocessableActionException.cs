using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Shared.Exception.Abstraction.Domain;

[Serializable]
public abstract class UnprocessableActionException : BaseException
{
    protected UnprocessableActionException()
    { }

    protected UnprocessableActionException(int code, string message) : base(code, message)
    { }

    protected UnprocessableActionException(string message) : base(message)
    {
    }

    protected UnprocessableActionException(Collection<KeyValuePair<string, string>> detail) : base(detail)
    {
    }

    protected UnprocessableActionException(string message, System.Exception innerException) : base(message, innerException)
    {
    }
    protected UnprocessableActionException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}