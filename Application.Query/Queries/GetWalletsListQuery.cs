using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Wallets;
using Domain.Core.Enums;
using Domain.Core.Helper;
using MediatR;
using Shared.MinIO.Contracts;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries
{
    public class GetWalletsListQuery : IRequest<List<GetWalletsListViewModel>>
    {
        public GetWalletsListQuery(int customerId, int? tenantId = null)
        {
            CustomerId = customerId;
            TenantId = tenantId;
        }
        public int CustomerId { get; }
        public int? TenantId { get; set; }
    }
    public class GetWalletsQueryHandler : BaseQueryHandler, IRequestHandler<GetWalletsListQuery, List<GetWalletsListViewModel>>
    {
        private readonly IWalletReadOnlyRepository _walletReadOnlyRepository;
        private readonly IAttachmentReadOnlyRepository _attachmentRepository;

        public GetWalletsQueryHandler(IWalletReadOnlyRepository walletReadOnlyRepository, IAttachmentReadOnlyRepository attachmentRepository)
        {
            _walletReadOnlyRepository = walletReadOnlyRepository;
            _attachmentRepository = attachmentRepository;
        }

        public async Task<List<GetWalletsListViewModel>> Handle(GetWalletsListQuery request, CancellationToken cancellationToken)
        {
            var wallets = await _walletReadOnlyRepository.GetWalletsAsync(request.CustomerId, request.TenantId);

            var attachments = await _attachmentRepository.GetAllFileReferencesAsync(EntityType.Plan, wallets.Select(c => c.PlanId.ToString()), cancellationToken);

            return wallets.Select(c => new GetWalletsListViewModel
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
        }
    }
}
