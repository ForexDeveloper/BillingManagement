using MediatR;
using System.Linq;
using System.Threading;
using Domain.Core.Enums;
using System.Threading.Tasks;
using Application.Query.Base;
using System.Collections.Generic;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Customers.Wallets;

namespace Application.Query.Queries;

public class GetCustomerBillsQuery : BasePaginatedListRequest, IRequest<GetCustomerBillsVm>
{
    public GetCustomerBillsQuery(int customerId, BillDateType dateType, BasePaginatedListRequest request, int? tenantId = null, int? walletId = null)
    {
        CustomerId = customerId;
        TenantId = tenantId;
        DateType = dateType;
        WalletName = request.SearchValue;
        PageSize = request.PageSize;
        PageIndex = request.PageIndex;
        SortColumn = request.SortColumn;
        SortDirection = request.SortDirection;
        WalletId = walletId;
    }
    public int CustomerId { get; set; }
    public int? TenantId { get; set; }
    public BillDateType DateType { get; set; }
    public string WalletName { get; set; }
    public int? WalletId { get; set; }


}

public class GetCustomerBillQueryHandler : IRequestHandler<GetCustomerBillsQuery, GetCustomerBillsVm>
{
    private readonly IBillingReadOnlyRepository _billingReadOnlyRepository;
    private readonly IAttachmentReadOnlyRepository _attachmentRepository;
    public GetCustomerBillQueryHandler(IBillingReadOnlyRepository billingReadOnlyRepository, IAttachmentReadOnlyRepository attachmentRepository)
    {
        _billingReadOnlyRepository = billingReadOnlyRepository;
        _attachmentRepository = attachmentRepository;
    }

    public async Task<GetCustomerBillsVm> Handle(GetCustomerBillsQuery request, CancellationToken cancellationToken)
    {
        var resultQueryModel = await _billingReadOnlyRepository.GetCustomerBillsAsync(request, cancellationToken);

        //var planLogos = await _attachmentRepository.GetListAsync(EntityType.Plan, resultQueryModel.Items.Select(x => x.PlanId).ToList());

        var planLogos = await _attachmentRepository.GetAllFileReferencesAsync(EntityType.Plan, resultQueryModel.Items.Select(x => x.PlanId.ToString()), cancellationToken);

        resultQueryModel.Items.ForEach(p =>
        {
            p.WalletLogoId = planLogos.GetValueOrDefault(p.PlanId.ToString());
        });

        return resultQueryModel.ToViewModel();
    }
}
