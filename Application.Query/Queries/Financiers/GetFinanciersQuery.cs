using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Financiers;
using Domain.Core.Enums;
using Domain.Core.Helper;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries.Financiers;

public class GetFinanciersQuery : BasePaginatedListRequest, IRequest<GetFinanciersVm>
{
    public int? TenantId { get; set; }
    public IdentityTypeEnum? PersonType { get; set; }
    public GetFinanciersQuery(int? tenantId, int pageIndex, int pageSize, string sortColumn, SortDirection? sortDirection, string searchValue, IdentityTypeEnum? personType)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
        SortColumn = sortColumn;
        SortDirection = sortDirection;
        SearchValue = searchValue;
        TenantId = tenantId;
        PersonType = personType;
    }

    public class GetFinanciersQueryHandler : BaseQueryHandler, IRequestHandler<GetFinanciersQuery, GetFinanciersVm>
    {
        private readonly IFinancierReadOnlyRepository _financierReadOnlyRepository;

        public GetFinanciersQueryHandler(IFinancierReadOnlyRepository financierReadOnlyRepository)
        {
            _financierReadOnlyRepository = financierReadOnlyRepository;
        }

        public async Task<GetFinanciersVm> Handle(GetFinanciersQuery request, CancellationToken cancellationToken)
        {
            var financiersModel = await _financierReadOnlyRepository.GetFinanciersAsync(request);

            return new GetFinanciersVm()
            {
                PageIndex = financiersModel.PageIndex,
                PageSize = financiersModel.PageSize,
                TotalCount = financiersModel.TotalCount,
                Items = financiersModel.Items.Select(x =>
                new GetFinancierVm
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
