using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Shared.Exception.Abstraction.Infrastructure;

[Serializable]
public abstract class InfrastructureBaseException : BaseException
{
    protected InfrastructureBaseException(int code, string message) : base(code, message)
    {
    }

    protected InfrastructureBaseException()
    {
    }

    protected InfrastructureBaseException(string message) : base(message)
    {
    }

    protected InfrastructureBaseException(Collection<KeyValuePair<string, string>> detail) : base(detail)
    {
    }

    protected InfrastructureBaseException(string message, System.Exception innerException) : base(message, innerException)
    {
    }
    protected InfrastructureBaseException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
