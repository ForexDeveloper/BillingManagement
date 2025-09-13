using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Shared.Exception.Abstraction.Infrastructure;

[Serializable]
public abstract class InfrastructureDbOperationException : InfrastructureBaseException
{
    public InfrastructureDbOperationException(int code, string message) : base(code, message)
    { }

    public InfrastructureDbOperationException()
    {
    }

    public InfrastructureDbOperationException(string message) : base(message)
    {
    }

    public InfrastructureDbOperationException(Collection<KeyValuePair<string, string>> detail) : base(detail)
    {
    }

    public InfrastructureDbOperationException(string message, System.Exception innerException) : base(message, innerException)
    {
    }
    private InfrastructureDbOperationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}