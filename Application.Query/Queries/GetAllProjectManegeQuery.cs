using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Tenants;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries
{
    public class GetAllProjectManagerQuery : IRequest<List<ProjectManagerViewModel>>
    {
        public int TenantId { get; set; }
        public GetAllProjectManagerQuery(int tenantId)
        {
            TenantId = tenantId;
        }
    }
    public class ProjectManagerQueryHandler : BaseQueryHandler, IRequestHandler<GetAllProjectManagerQuery, List<ProjectManagerViewModel>>
    {
        private readonly IProjectManagerReadOnlyRepository _projectManageReadOnlyRepository;
        public ProjectManagerQueryHandler(IProjectManagerReadOnlyRepository projectManageReadOnlyRepository)
        {
            _projectManageReadOnlyRepository = projectManageReadOnlyRepository;
        }

        public async Task<List<ProjectManagerViewModel>> Handle(GetAllProjectManagerQuery request, CancellationToken cancellationToken)
        {
            var result = await _projectManageReadOnlyRepository.GetByTenantIdAsync(request.TenantId);

            return result.Select(c => new ProjectManagerViewModel
            {
                Id = c.Id,
                FullName = c.FullName,
            }).ToList();
        }
    }
}