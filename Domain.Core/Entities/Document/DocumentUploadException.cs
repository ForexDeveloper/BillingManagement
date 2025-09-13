using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.Document
{
    public class DocumentUploadException : UnprocessableActionException
    {
        public DocumentUploadException(string message) : base($"{message}")
        {
        }
    }
}