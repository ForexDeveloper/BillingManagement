using Shared.Exception.Abstraction.Domain;

namespace Domain.Core.Entities.ProjectManegerAggregate.Exceptions
{
    public class ProjectManagerNotFoundException : NotFoundException
    {
        public ProjectManagerNotFoundException(string message) : base($"{message}")
        {

        }
    }
}