using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels;
using Application.Query.ViewModels.Facilitators;
using Domain.Core.Enums;
using Domain.Core.Helper;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries;

public class GetFacilitatorsQuery : BasePaginatedListRequest, IRequest<GetFacilitatorsVm>
{
    public int? TenantId { get; set; }
    public IdentityTypeEnum? PersonType { get; set; }

    public GetFacilitatorsQuery(int? tenantId, int pageIndex, int pageSize, string? sortColumn, SortDirection? sortDirection, string? searchValue, IdentityTypeEnum? personType)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
        SortColumn = sortColumn;
        SortDirection = sortDirection;
        SearchValue = searchValue;
        TenantId = tenantId;
        PersonType = personType;
    }

    public class GetFacilitatorsQueryHandler : BaseQueryHandler, IRequestHandler<GetFacilitatorsQuery, GetFacilitatorsVm>
    {
        private readonly IFacilitatorReadOnlyRepository _facilitatorReadOnlyRepository;

        public GetFacilitatorsQueryHandler(IFacilitatorReadOnlyRepository facilitatorReadOnlyRepository)
        {
            _facilitatorReadOnlyRepository = facilitatorReadOnlyRepository;
        }

        public async Task<GetFacilitatorsVm> Handle(GetFacilitatorsQuery request, CancellationToken cancellationToken)
        {
            var facilitatorsModel = await _facilitatorReadOnlyRepository.GetFacilitatorsAsync(request);

            return new GetFacilitatorsVm()
            {
                PageIndex = facilitatorsModel.PageIndex,
                PageSize = facilitatorsModel.PageSize,
                TotalCount = facilitatorsModel.TotalCount,
                Items = facilitatorsModel.Items.Select(x =>
                    new GetFacilitatorVm
                    {
                        Id = x.Id,
                        Name = x.Name,
                        TenantId = x.TenantId,
                        TenantName = x.TenantName,
                        PersonType = (IdentityTypeEnum)Enum.Parse(typeof(IdentityTypeEnum), x.PersonType.ToString()),
                        PersonTypeName = ((IdentityTypeEnum)Enum.Parse(typeof(IdentityTypeEnum), x.PersonType.ToString())).GetEnumDescription(),
                    }
                ).ToList()
            };
        }
    }
}
