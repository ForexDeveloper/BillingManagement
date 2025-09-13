using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.TenantMerchantContracts;
using Domain.Core.Entities.FinancialDocumentAggregate;
using Domain.Core.Entities.TenantMerchantContractAggregate;
using Domain.Core.Enums;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries
{
    public class GetTenantMerchantContractsQuery : BasePaginatedListRequest, IRequest<GetTenantMerchantContractsVm>
    {
        public int? TenantId { get; }
        public int? MerchantId { get; set; }
        public GuaranteeType? GuaranteeType { get; set; }
        public SettlementType? SettlementType { get; set; }
        public PaymentMethodType? PaymentMethodType { get; set; }

        public GetTenantMerchantContractsQuery(int? tenantId, int? merchantId, GuaranteeType? guaranteeType,
            SettlementType? settlementType,
            PaymentMethodType? paymentMethodType,
            int pageIndex, int pageSize, string sortColumn, SortDirection? sortDirection, string searchValue)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            SortColumn = sortColumn;
            SortDirection = sortDirection;
            SearchValue = searchValue;
            TenantId = tenantId;
            MerchantId = merchantId;
            GuaranteeType = guaranteeType;
            SettlementType = settlementType;
            PaymentMethodType = paymentMethodType;
        }
    }

    public class GetTenantMerchantContractsQueryHandler : IRequestHandler<GetTenantMerchantContractsQuery, GetTenantMerchantContractsVm>
    {
        private readonly ITenantMerchantContractReadOnlyRepository _tenantMerchantContractReadOnlyRepository;
        private readonly IFinancialDocumentRepository _financialDocumentRepository;
        private readonly ITenantMerchantContractRepository _tenantMerchantContractRepository;

        public GetTenantMerchantContractsQueryHandler(
            ITenantMerchantContractReadOnlyRepository tenantMerchantContractReadOnlyRepository,
            ITenantMerchantContractRepository tenantMerchantContractRepository,
            IFinancialDocumentRepository financialDocumentRepository)
        {
            _tenantMerchantContractReadOnlyRepository = tenantMerchantContractReadOnlyRepository;
            _tenantMerchantContractRepository = tenantMerchantContractRepository;
            _financialDocumentRepository = financialDocumentRepository;
        }

        public async Task<GetTenantMerchantContractsVm> Handle(GetTenantMerchantContractsQuery request, CancellationToken cancellationToken)
        {
            var contracts = await _tenantMerchantContractReadOnlyRepository.GetListAsync(request);

            var contractIdsHasTransaction = new List<int>();
            var contractIdsHasEndorsement = new List<int>();

            if (contracts.TotalCount > 0)
            {
                var contractIds = contracts.Items.Select(x => x.Id).ToList();
                contractIdsHasTransaction = await _financialDocumentRepository.GetTenantMerchantContractIdsHasTransaction(contractIds);
                contractIdsHasEndorsement = await _tenantMerchantContractRepository.GetContractIdsHasEndorsement(contractIds);
            }

            return new GetTenantMerchantContractsVm
            {
                PageIndex = contracts.PageIndex,
                PageSize = contracts.PageSize,
                TotalCount = contracts.TotalCount,
                Items = contracts.Items.Select(x =>
                    new TenantMerchantContractsVm
                    {
                        Id = x.Id,
                        TenantId = x.TenantId,
                        TenantName = x.TenantName,
                        MerchantId = x.MerchantId,
                        MerchantName = x.MerchantName,
                        ContractNumber = x.ContractNumber,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        CreatedDateTime = x.CreatedDateTime,
                        Status = x.Status,
                        IsEditable = !(contractIdsHasTransaction.Contains(x.Id) || contractIdsHasEndorsement.Contains(x.Id)),
                    }
                ).ToList()
            };
        }
    }
}
