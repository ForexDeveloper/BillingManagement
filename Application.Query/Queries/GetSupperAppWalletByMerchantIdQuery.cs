using MediatR;
using System.Linq;
using System.Threading;
using Domain.Core.Enums;
using System.Threading.Tasks;
using System.Collections.Generic;
using Application.Query.ViewModels.Wallets;
using Domain.Core.Entities.CustomerAggregate;
using Application.Query.ReadOnlyRepositoryContracts;
using Domain.Core.Entities.CustomerAggregate.Exceptions;
using Domain.Core.Entities.MerchantAggregate.Exceptions;

namespace Application.Query.Queries;

public record GetSuperAppWalletByMerchantIdQuery(int CustomerId, int MerchantId, int? TenantId)
    : IRequest<List<GetSuperAppWalletByMerchantIdViewModel>>
{
    public int CustomerId { get; } = CustomerId;

    public int MerchantId { get; set; } = MerchantId;

    public int? TenantId { get; set; } = TenantId;
}

public class GetSuperAppWalletByMerchantIdQueryHandler(
    ICustomerRepository customerRepository,
    IWalletReadOnlyRepository walletReadOnlyRepository,
    IMerchantReadOnlyRepository merchantReadOnlyRepository,
    IAttachmentReadOnlyRepository attachmentReadOnlyRepositoryRepository)
    : IRequestHandler<GetSuperAppWalletByMerchantIdQuery, List<GetSuperAppWalletByMerchantIdViewModel>>
{
    public async Task<List<GetSuperAppWalletByMerchantIdViewModel>> Handle(GetSuperAppWalletByMerchantIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await customerRepository.GetAsync(request.CustomerId);

        if (customer == null)
        {
            throw new CustomerNotFoundException("مشتری پیدا نشد");
        }

        var branch = await merchantReadOnlyRepository.GetBranchByMerchantIdAsync(request.MerchantId);

        if (branch == null)
        {
            throw new MerchantBranchNotFoundException("شعبه پذیرنده پیدا نشد");
        }

        var wallets = await walletReadOnlyRepository.GetWalletByMerchantIdAsync(request.CustomerId, branch.MerchantId, request.TenantId);

        var attachments = await attachmentReadOnlyRepositoryRepository.GetAllFileReferencesAsync(EntityType.Plan, wallets.Select(c => c.PlanId.ToString()), cancellationToken);

        return wallets.Select(p => new GetSuperAppWalletByMerchantIdViewModel
        {
            Id = p.Id,
            Title = p.Title,
            Balance = p.Balance,
            Type = p.WalletType,
            Status = p.WalletStatus,
            IsDefault = p.IsDefault,
            OrganizationTitle = p.OrganizationTitle,
            LogoId = attachments.GetValueOrDefault(p.PlanId.ToString())
        }).ToList();
    }
}