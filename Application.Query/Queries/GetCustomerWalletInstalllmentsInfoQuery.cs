using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Customers;
using Domain.Core.Entities.Shared.Exceptions;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries
{
    public class GetCustomerWalletInstalllmentsInfoQuery : IRequest<GetCustomerWalletInstallmentsInfoVm>
    {
        public GetCustomerWalletInstalllmentsInfoQuery(int id, int walletId, int? tenantId = null)
        {
            Id = id;
            WalletId = walletId;
            TenantId = tenantId;
        }
        public int Id { get; set; }
        public int? TenantId { get; set; }
        public int WalletId { get; set; }
    }

    public class GetCustomerWalletInstalllmentsInfoQueryHandler : BaseQueryHandler, IRequestHandler<GetCustomerWalletInstalllmentsInfoQuery, GetCustomerWalletInstallmentsInfoVm>
    {
        private readonly IWalletReadOnlyRepository _walletReadOnlyRepository;

        public GetCustomerWalletInstalllmentsInfoQueryHandler(IWalletReadOnlyRepository walletReadOnlyRepository)
        {
            _walletReadOnlyRepository = walletReadOnlyRepository;
        }

        public async Task<GetCustomerWalletInstallmentsInfoVm> Handle(GetCustomerWalletInstalllmentsInfoQuery request, CancellationToken cancellationToken)
        {
            var result = await _walletReadOnlyRepository.GetCustomerWalletInstallmentsInfoAsync(request.Id, request.WalletId, request.TenantId, cancellationToken);
            if (result == null)
                throw new ArgumentValidationException(nameof(request.WalletId), "شناسه کیف پول صحیح نیست.");

            return new GetCustomerWalletInstallmentsInfoVm()
            {
                FirstInstallmentDueDate = result.FirstInstallmentDueDate,
                LastInstallmentDueDate = result.LastInstallmentDueDate,
                PaidAmount = result.PaiedAmount,
                PaidInstallmentsCount = result.PaiedInstallmentCount,
                RemainingAmount = result.RemainingAmount,
                CustomerWalletInstallments = result.CustomerWalletInstallments.Select(c => new CustomerWalletInstallmentsVm
                {
                    DueDate = c.DueDate,
                    InstallmentIdentity = c.InstallmentIdentity,
                    InstallmentState = c.InstallmentState
                }).ToList()
            };
        }
    }
}
