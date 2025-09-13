using Shared.Exception.Abstraction;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Shared.Middlewares.Models;

[Serializable]
public class ModelStateValidationException : BaseException
{

    public ModelStateValidationException()
    {
    }

    public ModelStateValidationException(string message) : base(message)
    {
    }

    public ModelStateValidationException(string message, System.Exception innerException) : base(message, innerException)
    {
    }

    protected ModelStateValidationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    public ModelStateValidationException(Collection<KeyValuePair<string, string>> detail) : base(detail)
    {
    }
}