using Application.Query.Base;
using Application.Query.ViewModels.Tenants;
using MediatR;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System;
using Application.Query.ReadOnlyRepositoryContracts;

namespace Application.Query.Queries;

public class GetPurchasesListQuery :  BasePaginatedListRequest,IRequest<GetPurchasesListVm>
{
    public int TenantId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<int> WalletIds { get; set; } 
    public List<int> MerchantIds { get; set; }
    
}


public class GetPurchaseListQueryHandler
    : IRequestHandler<GetPurchasesListQuery, GetPurchasesListVm>
{
    private readonly IFinancialDocumentReadOnlyRepository _repository;

    public GetPurchaseListQueryHandler(IFinancialDocumentReadOnlyRepository repository)
    {
         _repository = repository;
    }

    public async Task<GetPurchasesListVm> Handle(
        GetPurchasesListQuery request,
        CancellationToken cancellationToken)
    {
        var purchases = await _repository.GetPurchasesListAsync(
            request,
            cancellationToken);

        return purchases;
    }
}