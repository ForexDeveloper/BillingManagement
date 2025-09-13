using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.Document
{
    public class DocumentNotFoundException : NotFoundException
    {
        public DocumentNotFoundException(string message) : base($"{message}")
        {
        }
    }
}