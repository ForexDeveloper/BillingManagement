using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.Shared.Exceptions;

public class ArgumentValidationException : ValidationException
{
    public ArgumentValidationException(string key, string message) : base(key, message) { }
}
