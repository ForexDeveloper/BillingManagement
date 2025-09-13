using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.Document
{
    public class DocumentInvalidDataException : UnprocessableActionException
    {
        public DocumentInvalidDataException(string message) : base($"{message}")
        {
        }
    }
}