using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels;
using Application.Query.ViewModels.WalletContracts;
using Domain.Core.Entities.Shared.Exceptions;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries
{
    public class GetWalletContractsCustomerQuery : BasePaginatedListRequest, IRequest<GetWalletContractCustomersVm>
    {
        public int WalletContractId { get; set; }
        public int? TenantId { get; set; }

        public GetWalletContractsCustomerQuery(int walletContractId,
        int pageIndex, int pageSize, string? sortColumn, SortDirection? sortDirection, string? searchValue, int? tenantId = null)
        {
            WalletContractId = walletContractId;
            PageIndex = pageIndex;
            PageSize = pageSize;
            SortColumn = sortColumn;
            SortDirection = sortDirection;
            SearchValue = searchValue;
            TenantId = tenantId;
        }
    }

    public class GetWalletContractsCustomerQueryHandler : IRequestHandler<GetWalletContractsCustomerQuery, GetWalletContractCustomersVm>
    {
        private readonly IWalletContractReadOnlyRepository _walletContractReadOnlyRepository;

        public GetWalletContractsCustomerQueryHandler(IWalletContractReadOnlyRepository walletContractReadOnlyRepository)
        {
            _walletContractReadOnlyRepository = walletContractReadOnlyRepository;
        }

        public async Task<GetWalletContractCustomersVm> Handle(GetWalletContractsCustomerQuery request, CancellationToken cancellationToken)
        {
            if (request.TenantId.HasValue)
            {
                var isWalletContractBelongToTenantAsync = await _walletContractReadOnlyRepository.IsWalletContractBelongToTenantAsync(request.WalletContractId, request.TenantId.Value);
                if (!isWalletContractBelongToTenantAsync)
                {
                    throw new ArgumentValidationException(nameof(request.WalletContractId), "قرارداد مورد نظر به مالک زیر ساخت تعلق ندارد.");
                }
            }
            var contracts = await _walletContractReadOnlyRepository.GetCustomersListAsync(request);

            return new GetWalletContractCustomersVm
            {
                PageIndex = contracts.PageIndex,
                PageSize = contracts.PageSize,
                TotalCount = contracts.TotalCount,
                Items = contracts.Items.Select(x =>
                    new WalletContractCustomerVm(x.Id, x.FullName)
                ).ToList()
            };
        }

    }
}
