using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.FinancialDocuments;
using Domain.Core.Entities.MerchantAggregate;
using Domain.Core.Entities.MerchantAggregate.Exceptions;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Helper;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries;

public class GetRefundFinancialDocumentByIdQuery : IRequest<RefundFinancialDocumentViewModel>
{
    public long Id { get; }
    public int? MerchantId { get; set; }
    public int TenantId { get; }
    public int? MerchantBranchId { get; }
    public bool IsMerchant { get; }

    public GetRefundFinancialDocumentByIdQuery(long id, int? merchantId, int tenantId, int? merchantBranchId = null, bool isMerchant = false)
    {
        Id = id;
        MerchantId = merchantId;
        TenantId = tenantId;
        MerchantBranchId = merchantBranchId;
        IsMerchant = isMerchant;
    }
}

public class GetRefundFinancialDocumentByIdQueryHandler : BaseQueryHandler, IRequestHandler<GetRefundFinancialDocumentByIdQuery, RefundFinancialDocumentViewModel>
{
    private readonly IFinancialDocumentReadOnlyRepository _financialDocumentReadOnlyRepository;
    private readonly IMerchantRepository _merchantRepository;

    public GetRefundFinancialDocumentByIdQueryHandler(IFinancialDocumentReadOnlyRepository financialDocumentReadOnlyRepository,
        IMerchantRepository merchantRepository)
    {
        _financialDocumentReadOnlyRepository = financialDocumentReadOnlyRepository;
        _merchantRepository = merchantRepository;
    }

    public async Task<RefundFinancialDocumentViewModel> Handle(GetRefundFinancialDocumentByIdQuery request, CancellationToken cancellationToken)
    {
        if (request.IsMerchant && (request.MerchantId == null || request.MerchantId == 0))
        {
            throw new ArgumentValidationException(nameof(request.MerchantId), "شناسه پذیرنده الزامی می باشد");
        }

        if (!request.IsMerchant && (request.MerchantBranchId == null || request.MerchantBranchId == 0))
        {
            throw new ArgumentValidationException(nameof(request.MerchantBranchId), "شناسه شعبه پذیرنده الزامی می باشد");
        }

        if (request.MerchantBranchId.HasValue && request.MerchantBranchId > 0)
        {
            var merchantBranch = await _merchantRepository.GetBranchAsync(request.MerchantBranchId.Value) ?? throw new MerchantBranchNotFoundException("شعبه پذیرنده یافت نشد");
            request.MerchantId = merchantBranch.MerchantId;
        }

        var result = await _financialDocumentReadOnlyRepository.GetFinanialDocumentRefundDetailByIdAsync(request.Id, request.MerchantId.Value, request.MerchantBranchId, request.TenantId);

        return new RefundFinancialDocumentViewModel
        {
            FinancialDocumentId = result.FinancialDocumentId,
            Amount = result.Amount,
            RemainAmount = result.RemainAmount,
            RefundDetails = result.RefundDetails.Select(x => new RefundFinancialDocumentDetailViewModel
            {
                RefundAmount = x.RefundAmount,
                RemainAmount = x.RemainAmount,
                RefundDateTime = x.RefundDateTime,
                RefundDescription = x.RefundDescription,
                RefundReason = x.RefundReason,
                RefundReasonTitle = x.RefundReason.GetEnumDescription()
            }).ToList()
        };
    }
}

