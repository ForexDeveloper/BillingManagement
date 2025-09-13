using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.WalletContracts;
using Application.Service.Contracts;
using Application.Service.Enums;
using Domain.Core.Entities.WalletContractAggregate;
using Domain.Core.Enums;
using Domain.Core.Helper;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries
{
    public class GetWalletContractsQuery : BasePaginatedListRequest, IRequest<GetWalletContractsVm>
    {
        public int? TenantId { get; set; }
        public List<int>? OrganizationIds { get; set; }
        public WalletContractStatus? Status { get; set; }
        public WalletContractEndDateType? EndDate { get; set; }

        public GetWalletContractsQuery(int? tenantId, List<int>? organizationIds, WalletContractStatus? status, WalletContractEndDateType? endDate,
        int pageIndex, int pageSize, string? sortColumn, SortDirection? sortDirection, string? searchValue)
        {
            TenantId = tenantId;
            OrganizationIds = organizationIds;
            Status = status;
            EndDate = endDate;
            PageIndex = pageIndex;
            PageSize = pageSize;
            SortColumn = sortColumn;
            SortDirection = sortDirection;
            SearchValue = searchValue;
        }
    }

    public class GetWalletContractsQueryHandler : IRequestHandler<GetWalletContractsQuery, GetWalletContractsVm>
    {
        private readonly IWalletContractReadOnlyRepository _walletContractReadOnlyRepository;
        private readonly IWalletContractRepository _walletContractRepository;
        private readonly IWalletContractService _walletContractService;

        public GetWalletContractsQueryHandler(IWalletContractReadOnlyRepository walletContractReadOnlyRepository,
            IWalletContractRepository walletContractRepository,
            IWalletContractService walletContractService)
        {
            _walletContractReadOnlyRepository = walletContractReadOnlyRepository;
            _walletContractRepository = walletContractRepository;
            _walletContractService = walletContractService;
        }

        public async Task<GetWalletContractsVm> Handle(GetWalletContractsQuery request, CancellationToken cancellationToken)
        {
            var contracts = await _walletContractReadOnlyRepository.GetRootParentListAsync(request);
            List<int> contractIdsHasCashWallet = new List<int>();
            if (contracts.Items.Count > 0)
            {
                contractIdsHasCashWallet = await _walletContractReadOnlyRepository.GetWalletContractIdsHasCashWalletAsync(contracts.Items.Where(x => x.GrantingProcessId == null).Select(x => x.Id).ToList());
            }

            return new GetWalletContractsVm
            {
                PageIndex = contracts.PageIndex,
                PageSize = contracts.PageSize,
                TotalCount = contracts.TotalCount,
                Items = contracts.Items.Select(x =>
                    new WalletContractsVm
                    {
                        Id = x.Id,
                        TenantId = x.TenantId,
                        TenantName = x.TenantName,
                        ContractNumber = x.ContractNumber,
                        Status = x.Status,
                        StatusTitle = x.StatusTitle,
                        OrganizationId = x.OrganizationId,
                        OrganizationTitle = x.OrganizationTitle,
                        EditDateTime = x.EditDateTime,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        DisplayEndDate = _walletContractService.CreateWalletContractDisplayEndDate(x.EndDate),
                        Plans = x.Plans.Select(x => new WalletContractPlanVm(x.Id, x.PlanId, x.PlanTitle)).ToList(),
                        LastEndorsementStatus = (x.LastEndorsementStatus == 0 || x.LastEndorsementStatus == null) ? null : x.LastEndorsementStatus,
                        LastEndorsementStatusTitle = x.LastEndorsementStatus.GetEnumDescription(),
                        WalletContractOriginType = GetWalletContractOriginType(x.Id, x.GrantingProcessId, contractIdsHasCashWallet)
                    }
                ).ToList()
            };
        }

        private WalletContractOriginType GetWalletContractOriginType(int contractId, int? grantingProcessId, List<int> contractIdsHasCashWallet)
        {
            if (grantingProcessId.HasValue)
            {
                return WalletContractOriginType.Granting;
            }

            if (contractIdsHasCashWallet.Contains(contractId))
            {
                return WalletContractOriginType.CashWallet;
            }

            return WalletContractOriginType.PanelAdmin;
        }
    }
}
