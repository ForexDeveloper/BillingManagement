using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Customers.Wallets;
using Application.Query.ViewModels.Wallets;
using Domain.Core.Enums;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries
{
    public class GetWalletsPaginatedListQuery : BasePaginatedListRequest, IRequest<GetCustomerWalletsVm>
    {
        public GetWalletsPaginatedListQuery(int customerId, int? tenantId, BasePaginatedListRequest request)
        {
            CustomerId = customerId;
            TenantId = tenantId;
            PageSize = request.PageSize;
            PageIndex = request.PageIndex;
            SortColumn = request.SortColumn;
            SortDirection = request.SortDirection;
        }
        public int CustomerId { get; }
        public int? TenantId { get; set; }
    }

    public class GetWalletsPaginatedListQueryHandler : BaseQueryHandler, IRequestHandler<GetWalletsPaginatedListQuery, GetCustomerWalletsVm>
    {
        private readonly IWalletReadOnlyRepository _walletReadOnlyRepository;
        private readonly IAttachmentReadOnlyRepository _attachmentRepository;

        public GetWalletsPaginatedListQueryHandler(IWalletReadOnlyRepository walletReadOnlyRepository, IAttachmentReadOnlyRepository attachmentRepository)
        {
            _walletReadOnlyRepository = walletReadOnlyRepository;
            _attachmentRepository = attachmentRepository;
        }

        public async Task<GetCustomerWalletsVm> Handle(GetWalletsPaginatedListQuery request, CancellationToken cancellationToken)
        {
            var wallets = await _walletReadOnlyRepository.GetWalletsAsync(request);

            var attachments = await _attachmentRepository.GetAllFileReferencesAsync(EntityType.Plan, wallets.Items.Select(c => c.PlanId.ToString()), cancellationToken);

            var result = wallets.Items.Select(c => new GetWalletsListViewModel
            {
                Balance = c.Balance,
                Type = c.WalletType,
                Id = c.Id,
                Title = c.Title,
                OrganizationTitle = c.OrganizationTitle,
                LogoId = attachments.GetValueOrDefault(c.PlanId.ToString()),
                Status = c.WalletStatus,
                IsDefault = c.IsDefault
            }).ToList();

            return new GetCustomerWalletsVm
            {
                Items = result,
                PageIndex = wallets.PageIndex,
                PageSize = wallets.PageSize,
                TotalCount = wallets.TotalCount
            };
        }
    }
}
