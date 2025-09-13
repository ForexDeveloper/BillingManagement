using Application.Query.Base;
using MediatR;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.FinancialDocuments;
using System.Linq;

namespace Application.Query.Queries;

public class GetMerchantPurchasesQuery : BasePaginatedListRequest, IRequest<GetMerchantPurchasesViewModel>
{
    public GetMerchantPurchasesQuery(BasePaginatedListRequest query, DateTime? startDate, DateTime? endDate, List<int> wallets,
        List<int> merchantBranchIds, int? merchantId = null, int? tenantId=null)
    {
        PageIndex = query.PageIndex;
        PageSize = query.PageSize;
        SortColumn = query.SortColumn;
        SortDirection = query.SortDirection;
        SearchValue = query.SearchValue;
        StartDate = startDate;
        EndDate = endDate;
        Wallets = wallets;
        MerchantBranchIds = merchantBranchIds;
        MerchantId = merchantId;
        TenantId = tenantId;
    }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<int> Wallets { get; set; }
    public List<int> MerchantBranchIds { get; set; }
    public int? MerchantId { get; set; }
    public int? TenantId { get; set; }

}


public class GetMerchantPurchasesQueryHandler  : IRequestHandler<GetMerchantPurchasesQuery, GetMerchantPurchasesViewModel>
{
    private readonly IFinancialDocumentReadOnlyRepository _repository;

    public GetMerchantPurchasesQueryHandler(IFinancialDocumentReadOnlyRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetMerchantPurchasesViewModel> Handle(GetMerchantPurchasesQuery request, CancellationToken cancellationToken)
    {
        var purchases = await _repository.GetPurchasesByMerchantIdAsync(request);

        return new GetMerchantPurchasesViewModel
        {
            PageIndex = purchases.PageIndex,
            PageSize = purchases.PageSize,
            TotalCount = purchases.TotalCount,
            Items = purchases.Items.Select(x =>
            new GetMerchantPurchaseViewModel
            {
                Id = x.Id,
               BranchName = x.BranchName,
               TotalAmount = x.TotalAmount,
               ReferenceNumber = x.PaymentId,
               CreateDateTime = x.CreateDateTime,
               Mobile = x.Mobile    
            }).ToList()
        };
    }
}