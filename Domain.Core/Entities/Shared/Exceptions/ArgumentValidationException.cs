using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.Shared.Exceptions;

public class ArgumentValidationException(string key, string message) : ValidationException(key, message);