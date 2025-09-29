using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.TenantPlatfromContracts;
using Domain.Core.Entities.FinancialDocumentAggregate;
using Domain.Core.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries.TenantPlatfromContracts
{
    public class GetTenantPlatformContractsQuery : BasePaginatedListRequest, IRequest<GetTenantPlatformContractsVm>
    {
        public string ContractNumber { get; set; }
        public int? TenantId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public FeeCalculationType? FeeCalculationType { get; set; }
        public CommissionCalculationType? CommissionCalculationType { get; set; }

        public GetTenantPlatformContractsQuery(string contractNumber, int? tenantId, DateTime? startDate, DateTime? endDate,
            FeeCalculationType? feeCalculationType,
            CommissionCalculationType? commissionCalculationType,
        int pageIndex, int pageSize, string sortColumn, SortDirection? sortDirection, string searchValue)
        {
            TenantId = tenantId;
            ContractNumber = contractNumber;
            TenantId = tenantId;
            StartDate = startDate;
            EndDate = endDate;
            FeeCalculationType = feeCalculationType;
            CommissionCalculationType = commissionCalculationType;
            PageIndex = pageIndex;
            PageSize = pageSize;
            SortColumn = sortColumn;
            SortDirection = sortDirection;
            SearchValue = searchValue;
        }
    }

    public class GetTenantPlatformContractsQueryHandler : IRequestHandler<GetTenantPlatformContractsQuery, GetTenantPlatformContractsVm>
    {
        private readonly ITenantPlatformContractReadOnlyRepository _tenantPlatformContractReadOnlyRepository;
        private readonly IFinancialDocumentRepository _financialDocumentRepository;

        public GetTenantPlatformContractsQueryHandler(
            ITenantPlatformContractReadOnlyRepository tenantPlatformContractReadOnlyRepository,
            IFinancialDocumentRepository financialDocumentRepository)
        {
            _tenantPlatformContractReadOnlyRepository = tenantPlatformContractReadOnlyRepository;
            _financialDocumentRepository = financialDocumentRepository;
        }

        public async Task<GetTenantPlatformContractsVm> Handle(GetTenantPlatformContractsQuery request, CancellationToken cancellationToken)
        {
            var contracts = await _tenantPlatformContractReadOnlyRepository.GetListAsync(request);
            var contractIdsHasTransaction = new List<int>();
            if (contracts.TotalCount > 0)
            {
                var contractIds = contracts.Items.Select(x => x.Id).ToList();
                contractIdsHasTransaction = await _financialDocumentRepository.GetTenantPlatformContractIdsHasTransaction(contractIds);
            }

            return new GetTenantPlatformContractsVm
            {
                PageIndex = contracts.PageIndex,
                PageSize = contracts.PageSize,
                TotalCount = contracts.TotalCount,
                Items = contracts.Items.Select(x =>
                    new TenantPlatformContractsVm
                    {
                        Id = x.Id,
                        TenantId = x.TenantId,
                        TenantName = x.TenantIName,
                        ContractNumber = x.ContractNumber,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        BrandName = x.BrandName,
                        CreditProjectName = x.CreditProjectName,
                        InternalProjectManagerName = x.ProjectManagerName,
                        Status = x.Status,
                        IsEditable = contractIdsHasTransaction.Contains(x.Id),
                    }
                ).ToList()
            };
        }

    }
}
