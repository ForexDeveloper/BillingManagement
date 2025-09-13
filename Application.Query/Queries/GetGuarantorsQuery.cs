using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels;
using Application.Query.ViewModels.Guarantors;
using Domain.Core.Enums;
using Domain.Core.Helper;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries;

public class GetGuarantorsQuery : BasePaginatedListRequest, IRequest<GetGuarantorsVm>
{
    public int? TenantId { get; set; }
    public IdentityTypeEnum? PersonType { get; set; }

    public GetGuarantorsQuery(int? tenantId, int pageIndex, int pageSize, string? sortColumn, SortDirection? sortDirection, string? searchValue, IdentityTypeEnum? personType)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
        SortColumn = sortColumn;
        SortDirection = sortDirection;
        SearchValue = searchValue;
        TenantId = tenantId;
        PersonType = personType;
    }

    public class GetGuarantorsQueryHandler : BaseQueryHandler, IRequestHandler<GetGuarantorsQuery, GetGuarantorsVm>
    {
        private readonly IGuarantorReadOnlyRepository _guarantorReadOnlyRepository;

        public GetGuarantorsQueryHandler(IGuarantorReadOnlyRepository guarantorReadOnlyRepository)
        {
            _guarantorReadOnlyRepository = guarantorReadOnlyRepository;
        }

        public async Task<GetGuarantorsVm> Handle(GetGuarantorsQuery request, CancellationToken cancellationToken)
        {
            var guarantorsModel = await _guarantorReadOnlyRepository.GetGuarantorsAsync(request);

            return new GetGuarantorsVm()
            {
                PageIndex = guarantorsModel.PageIndex,
                PageSize = guarantorsModel.PageSize,
                TotalCount = guarantorsModel.TotalCount,
                Items = guarantorsModel.Items.Select(x =>
                new GetGuarantorVm
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
