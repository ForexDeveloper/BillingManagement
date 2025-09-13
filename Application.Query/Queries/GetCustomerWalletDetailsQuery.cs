using Application.Query.Base;
using Application.Query.ViewModels.Wallets;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Application.Query.ReadOnlyRepositoryContracts;
using Domain.Core.Entities.Shared.Exceptions;

namespace Application.Query.Queries;


public class GetCustomerWalletDetailsQuery : IRequest<CutomerWalletDetailsViewModel>
{
    public int CustomerId { get; set; }
    public int WalletId { get; set; }
    public int? TenantId { get; }

    public GetCustomerWalletDetailsQuery(int customerId, int walletId, int? tenantId = null)
    {
        CustomerId = customerId;
        WalletId = walletId;
        TenantId = tenantId;
    }


}
public class GetCustomerWalletDetailsQueryHandler : BaseQueryHandler, IRequestHandler<GetCustomerWalletDetailsQuery, CutomerWalletDetailsViewModel>
{
    private readonly IWalletReadOnlyRepository _walletReadOnlyRepository;

    public GetCustomerWalletDetailsQueryHandler(IWalletReadOnlyRepository walletReadOnlyRepository)
    {
        _walletReadOnlyRepository = walletReadOnlyRepository;
    }

    public async Task<CutomerWalletDetailsViewModel> Handle(GetCustomerWalletDetailsQuery request,
     CancellationToken cancellationToken)
    {
        var result = await _walletReadOnlyRepository.GetCustomerWalletDetailsAsync(request.TenantId, request.CustomerId, request.WalletId);
        if (result == null)
            throw new ArgumentValidationException(nameof(request.WalletId), "شناسه نامعتبر است.");

        return new CutomerWalletDetailsViewModel
        {
            Balance = result.Balance,
            CreateDateTime = result.CreateDateTime,
            InitialAmount = result.InitialAmount,
            InstallmentsCount = result.InstallmentsCount,
            Status = result.Status,
            Type = result.Type,
            TermsAndConditions = result.TermsAndConditions,
            OrganizationName = result.OrganizationName,
        };
    }
}