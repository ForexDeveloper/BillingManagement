using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Merchants;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries.Merchants;

public class GetMerchantListQuery : BasePaginatedListRequest, IRequest<List<GetMerchantListVm>>
{
    public int TenantId { get; set; }
    public GetMerchantListQuery(int tenantId)
    {
        TenantId = tenantId;
    }

    public class MerchantListQueryHandler : BaseQueryHandler, IRequestHandler<GetMerchantListQuery, List<GetMerchantListVm>>
    {
        private readonly IMerchantReadOnlyRepository _merchantReadOnlyRepository;
        public MerchantListQueryHandler(IMerchantReadOnlyRepository merchantReadOnlyRepository)
        {
            _merchantReadOnlyRepository = merchantReadOnlyRepository;
        }

        public async Task<List<GetMerchantListVm>> Handle(GetMerchantListQuery request, CancellationToken cancellationToken)
        {
            var result = await _merchantReadOnlyRepository.GetListAsync(request.TenantId);

            return result.Select(x => new GetMerchantListVm
            {
                Id = x.Id,
                Title = x.Title,
            }).ToList();
        }
    }

}
