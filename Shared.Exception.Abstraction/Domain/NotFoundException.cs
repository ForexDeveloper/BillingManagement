using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Shared.Exception.Abstraction.Domain;

[Serializable]
public abstract class NotFoundException : BaseException
{
    protected NotFoundException()
    { }

    protected NotFoundException(int code, string message) : base(code, message)
    { }

    protected NotFoundException(string message) : base(message)
    {
    }

    protected NotFoundException(Collection<KeyValuePair<string, string>> detail) : base(detail)
    {
    }

    protected NotFoundException(string message, System.Exception innerException) : base(message, innerException)
    {
    }
    protected NotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}