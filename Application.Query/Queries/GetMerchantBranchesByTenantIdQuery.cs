using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Guarantors;
using Application.Query.ViewModels.Merchants;
using Domain.Core.Entities.GuarantorAggregate.Exceptions;
using Domain.Core.Enums;
using Domain.Core.Helper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries;

public class GetMerchantBranchesByTenantIdQuery : IRequest<List<GetMerchantBranchViewModel>>
{
    public int MerchantId { get; }
    public int? TenantId { get; set; }

    public GetMerchantBranchesByTenantIdQuery(int merchantId, int? tenantId = null)
    {
        MerchantId = merchantId;
        TenantId = tenantId;
    }
}

public class GetGuarantorByTenantIdQueryHandler : BaseQueryHandler, IRequestHandler<GetMerchantBranchesByTenantIdQuery, List<GetMerchantBranchViewModel>>
{
    private readonly IMerchantReadOnlyRepository _merchantReadOnlyRepository;

    public GetGuarantorByTenantIdQueryHandler(IMerchantReadOnlyRepository merchantReadOnlyRepository)
    {
        _merchantReadOnlyRepository = merchantReadOnlyRepository;
    }

    public async Task<List<GetMerchantBranchViewModel>> Handle(GetMerchantBranchesByTenantIdQuery request, CancellationToken cancellationToken)
    {
        var branches = await _merchantReadOnlyRepository.GetMerchantBranchesByTenantIdAsync(request.MerchantId, request.TenantId);
        var result = branches.Select(c => new GetMerchantBranchViewModel
        {
            Title = c.Title,
            Id = c.Id
        }).ToList();
        return result;
    }
}
