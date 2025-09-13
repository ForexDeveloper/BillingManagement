using Application.Query.Queries;
using Application.Query.QueryModels;
using Application.Query.ReadOnlyRepositoryContracts;
using Domain.Core.Entities.CustomerAggregate;
using Domain.Core.Entities.WalletContractAggregate;
using Domain.Core.Enums;
using Domain.Core.Helper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repository.EfCore.ReadonlyRepositories
{
    public class WalletContractReadOnlyRepository : IWalletContractReadOnlyRepository
    {
        private readonly ReadonlyApplicationDbContext _readonlyApplicationDbContext;

        public WalletContractReadOnlyRepository(ReadonlyApplicationDbContext readonlyApplicationDbContext)
        {
            _readonlyApplicationDbContext = readonlyApplicationDbContext;
        }

        public async Task<WalletContractGetByIdQueryModel> GetByIdAsync(int id, int? tenantId)
        {
            var contract = await _readonlyApplicationDbContext.WalletContracts
                .Include(x => x.TenantIpgSetting)
                .Where(x => (x.Id == id || x.ParentId == id) && (!tenantId.HasValue || x.TenantId == tenantId))
                .Select(x => new WalletContractGetByIdQueryModel()
                {
                    Id = x.Id,
                    OrganizationId = x.OrganizationId,
                    OrganizationTitle = x.Organization.Title,
                    TenantId = x.TenantId,
                    TenantName = x.Tenant.Title,
                    ContractNumber = x.ContractNumber,
                    Status = x.Status,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    Rejections = x.WalletContractRejectionReasons,
                    TenantIpgSettingId = x.TenantIpgSettingId,
                    GrantingProcessId = x.GrantingProcessId,
                    RootParentId = x.RootParentId,
                    TenantIpgSettingName = x.TenantIpgSetting != null ? x.TenantIpgSetting.Title : null,
                    WalletContractPlans = x.WalletContractPlans.Select(p => new WalletContractPlanQueryModel(p.Id, p.PlanId, p.Plan.Title)).ToList(),
                    WalletContractGuarantors = x.WalletContractGuarantors.Select(g => new WalletContractGuarantorQueryModel(g.Id, g.WalletContractId, g.GuarantorId, g.Guarantor.Name, g.PortionTypes,
                        g.CommissionCalculationType, g.FixedAmountCommission, g.FixedPercentageCommission, g.TransactionMinCommissionAmount, g.TransactionMaxCommissionAmount, g.PeriodMinCommissionAmount, g.PeriodMaxCommissionAmount, g.TieredCommissions, g.PaymentMethodType)).ToList(),
                    WalletContractFinanciers = x.WalletContractFinanciers.Select(g => new WalletContractFinancierQueryModel(g.Id, g.WalletContractId, g.FinancierId, g.Financier.Name, g.PortionTypes,
                        g.CommissionCalculationType, g.FixedAmountCommission, g.FixedPercentageCommission, g.TransactionMinCommissionAmount, g.TransactionMaxCommissionAmount, g.PeriodMinCommissionAmount, g.PeriodMaxCommissionAmount, g.TieredCommissions, g.PaymentMethodType)).ToList(),
                    WalletContractFacilitators = x.WalletContractFacilitators.Select(g => new WalletContractFacilitatorQueryModel(g.Id, g.WalletContractId, g.FacilitatorId, g.Facilitator.Name, g.PortionTypes,
                        g.CommissionCalculationType, g.FixedAmountCommission, g.FixedPercentageCommission, g.TransactionMinCommissionAmount, g.TransactionMaxCommissionAmount, g.PeriodMinCommissionAmount, g.PeriodMaxCommissionAmount, g.TieredCommissions, g.PaymentMethodType)).ToList(),
                })
                .FirstOrDefaultAsync();

            return contract;
        }
        public async Task<bool> HasCashWalletContractByIdAsync(int id)
        {
            return await _readonlyApplicationDbContext.WalletContractPlans.AnyAsync(c => c.WalletContractId == id
            && c.Plan.WalletConfiguration.WalletTypeId == Domain.Core.Enums.WalletType.Cash);
        }

        public async Task<List<int>> GetWalletContractIdsHasCashWalletAsync(List<int> walletContractIds)
        {
            var matchedIds = await _readonlyApplicationDbContext.WalletContractPlans
                .Where(c => walletContractIds.Contains(c.WalletContractId) &&
                            c.Plan.WalletConfiguration.WalletTypeId == WalletType.Cash)
                .Select(c => c.WalletContractId)
                .Distinct()
                .ToListAsync();

            return matchedIds;
        }


        public async Task<WalletContractsQueryModel> GetRootParentListAsync(GetWalletContractsQuery request)
        {
            var query = _readonlyApplicationDbContext.WalletContracts.Where(x => x.ParentId == null && x.RootParentId == null)
                .Include(x => x.Organization)
                .Include(x => x.Tenant)
                .Include(x => x.WalletContractPlans).ThenInclude(x => x.Plan)
                .AsQueryable();

            if (request.TenantId.HasValue)
            {
                query = query.Where(x => x.TenantId == request.TenantId);
            }

            if (request.OrganizationIds != null && request.OrganizationIds.Count > 0)
            {
                query = query.Where(x => request.OrganizationIds.Contains(x.OrganizationId));
            }


            if (request.Status.HasValue)
            {
                query = query.Where(x => x.Status == request.Status);
            }

            if (request.EndDate.HasValue)
            {
                query = FilterByWalletContractEndDate(request, query);
            }


            if (!string.IsNullOrWhiteSpace(request.SearchValue))
            {
                request.SearchValue = request.SearchValue.Trim();
                query = query.Where(x =>
                    x.ContractNumber.ToString().Contains(request.SearchValue) ||
                    x.Organization.Title.Contains(request.SearchValue)
                );
            }

            if (!string.IsNullOrWhiteSpace(request.SortColumn))
            {
                query = request.SortColumn.ToLower() switch
                {
                    "organizationId" => request.SortDirection == Application.Query.Base.SortDirection.Ascending ?
                        query.OrderBy(x => x.OrganizationId) : query.OrderByDescending(x => x.OrganizationId),

                    _ => query.OrderByDescending(x => x.Id)
                };
            }
            else
            {
                query = query.OrderByDescending(x => x.Id);
            }

            var totalCounts = await query.CountAsync();

            var contracts = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                 .Take(request.PageSize)
                .Select(x => new WalletContractQueryModel
                {
                    Id = x.Id,
                    TenantId = x.TenantId,
                    TenantName = x.Tenant.Title,
                    ContractNumber = x.ContractNumber,
                    Status = x.Status,
                    StatusTitle = x.Status.GetEnumDescription(),
                    OrganizationId = x.OrganizationId,
                    OrganizationTitle = x.Organization.Title,
                    EditDateTime = x.EditDateTime,
                    GrantingProcessId = x.GrantingProcessId,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    ParentId = x.ParentId,
                    Plans = x.WalletContractPlans.Select(p => new WalletContractPlanQueryModel(p.Id, p.PlanId, p.Plan.Title)).ToList(),
                    LastEndorsementStatus = _readonlyApplicationDbContext.WalletContracts
                        .Where(c => c.RootParentId == x.Id).OrderByDescending(c => c.Id).Select(c => c.Status).FirstOrDefault()
                })
                .ToListAsync();

            return new WalletContractsQueryModel()
            {
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                Items = contracts,
                TotalCount = totalCounts
            };
        }

        public async Task<WalletContractCustomersQueryModel> GetCustomersListAsync(GetWalletContractsCustomerQuery request)
        {
            var query = _readonlyApplicationDbContext.WalletContractBusinessIdentities
                .Include(x => x.BusinessIdentity)
                .Where(x => x.WalletContractId == request.WalletContractId);

            if (!string.IsNullOrWhiteSpace(request.SearchValue))
            {
                request.SearchValue = request.SearchValue.Trim();
                query = query.Where(x =>
                    (x.BusinessIdentity as Customer).FullName.Contains(request.SearchValue)
                );
            }

            if (!string.IsNullOrWhiteSpace(request.SortColumn))
            {
                query = request.SortColumn.ToLower() switch
                {
                    "customerId" => request.SortDirection == Application.Query.Base.SortDirection.Ascending ?
                        query.OrderBy(x => x.BusinessIdentity) : query.OrderByDescending(x => x.BusinessIdentityId),

                    _ => query.OrderByDescending(x => x.Id)
                };
            }
            else
            {
                query = query.OrderByDescending(x => x.Id);
            }

            var totalCounts = await query.CountAsync();

            var contracts = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                 .Take(request.PageSize)
                .Select(x => new WalletContractCustomerQueryModel
                {
                    Id = x.BusinessIdentityId,
                    FullName = (x.BusinessIdentity as Customer).FullName
                })
                .ToListAsync();


            return new WalletContractCustomersQueryModel()
            {
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                Items = contracts,
                TotalCount = totalCounts
            };
        }

        public async Task<List<WalletContractRejectionQueryModel>> GetRejectionListAsync(int id)
        {
            return await _readonlyApplicationDbContext.WalletContractRejectionReasons
                .Where(x => x.WalletContractId == id)
                .Select(x => new WalletContractRejectionQueryModel(x.Id, x.Reason, x.CreatedDateTime))
                .ToListAsync();
        }

        public async Task<bool> IsWalletContractBelongToTenantAsync(int walletContractId, int tenantId)
        {
            return await _readonlyApplicationDbContext.WalletContracts
                .Include(x => x.WalletContractPlans)
                .ThenInclude(x => x.Plan)
                .ThenInclude(x => x.WalletConfiguration)
                .AnyAsync(x => x.Id == walletContractId && x.TenantId == tenantId);
        }

        public async Task<int?> GetTenantIpgSettingIdAsync(int id, int tenantId)
        {
            var contract = await _readonlyApplicationDbContext.WalletContracts
                .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);

            return contract?.TenantIpgSettingId;
        }

        private IQueryable<Domain.Core.Entities.WalletContractAggregate.WalletContract> FilterByWalletContractEndDate(GetWalletContractsQuery request, IQueryable<Domain.Core.Entities.WalletContractAggregate.WalletContract> query)
        {
            switch (request.EndDate)
            {
                case Domain.Core.Enums.WalletContractEndDateType.LessThanOneDay:
                    {
                        query = query.Where(x => x.EndDate.Value.Date <= DateTime.Today.Date);
                        break;
                    }

                case Domain.Core.Enums.WalletContractEndDateType.LessThanTwoWeeks:
                    {
                        query = query.Where(x => x.EndDate.Value.Date < DateTime.Now.AddDays(14));
                        break;
                    }

                case Domain.Core.Enums.WalletContractEndDateType.LessThanOneMonth:
                    {
                        query = query.Where(x => x.EndDate.Value.Date < DateTime.Now.AddDays(31));
                        break;
                    }

                case Domain.Core.Enums.WalletContractEndDateType.LessThanSixMonths:
                    {
                        query = query.Where(x => x.EndDate.Value.Date < DateTime.Now.AddDays(186));
                        break;
                    }
            }

            return query;
        }

        public async Task<List<WalletContractChildQueryModel>> GetEndorsementsByRootParentIdListAsync(int rootParentId, int? tenantId)
        {
            var endorsements = await _readonlyApplicationDbContext.WalletContracts
                .Where(c => (c.Id == rootParentId || c.RootParentId == rootParentId) && (!tenantId.HasValue || c.TenantId == tenantId))
                .Select(c => new WalletContractChildQueryModel
                {
                    Id = c.Id,
                    RootParentId = c.RootParentId,
                    ChangeStatusDate = c.ChangeStatusDate,
                    Status = c.Status,
                    ContractNumber = c.ContractNumber,
                    EndDate = c.EndDate,
                })
                .ToListAsync();

            return endorsements;
        }

        public async Task<WalletContractWithSameRootParentIdQueryModel> GetForCloneByContractIdAsync(int id, int tenantId)
        {
            var contract = await _readonlyApplicationDbContext.WalletContracts
                .Where(x => x.Id == id && x.TenantId == tenantId)
                .Select(x => new WalletContractWithSameRootParentIdQueryModel
                {
                    Id = x.Id,
                    TenantId = x.TenantId,
                    RootParentId = x.RootParentId,
                    Status = x.Status,
                    OrganizationId = x.OrganizationId,
                    PlanIds = x.WalletContractPlans.Select(p => p.PlanId).ToList(),
                })
                .FirstOrDefaultAsync();

            return contract;
        }

        public async Task<List<WalletContractChildQueryModel>> GetNotRejectedEndorsementsListAsync(int contractId, int? rootParentId, int tenantId)
        {
            var query = _readonlyApplicationDbContext.WalletContracts.AsQueryable();
            if (rootParentId == null)
            {
                query = query.Where(x => (x.Id == contractId || x.RootParentId == contractId) && x.Status != WalletContractStatus.Reject);
            }
            else
            {
                query = query.Where(x => (x.Id == contractId || x.RootParentId == contractId || x.Id == rootParentId) && x.Status != WalletContractStatus.Reject);
            }

            var endorsements = await query
               .Select(c => new WalletContractChildQueryModel
               {
                   Id = c.Id,
                   RootParentId = c.RootParentId,
                   ChangeStatusDate = c.ChangeStatusDate,
                   Status = c.Status,
                   ContractNumber = c.ContractNumber,
                   EndDate = c.EndDate,
               })
               .ToListAsync();

            return endorsements;
        }

        public async Task<bool> ExistsActiveOrDeactiveContractWithIdGreaterThan(int contractId, int? rootParentId, int tenantId)
        {
            return await _readonlyApplicationDbContext.WalletContracts
               .AnyAsync(c => (c.Id > contractId && (!rootParentId.HasValue || c.RootParentId == rootParentId)) && c.TenantId == tenantId && (c.Status == WalletContractStatus.Active || c.Status == WalletContractStatus.DeActive));
        }

        public Task<WalletContract> GetActiveContractWithIdSmallerThan(int contractId, int? rootParentId, int tenantId)
        {
            return _readonlyApplicationDbContext.WalletContracts.FirstOrDefaultAsync(x => (x.Id < contractId || x.RootParentId == rootParentId || x.Id == rootParentId) && x.TenantId == tenantId && x.Status == WalletContractStatus.Active);
        }

        public async Task<bool> GetActiveContractByContractIdAsync(int id, int tenantId)
        {
            var contractExists = await _readonlyApplicationDbContext.WalletContracts
                .AnyAsync(x => x.Id == id && x.TenantId == tenantId && x.Status == WalletContractStatus.Active && !x.IsDeleted && x.EndDate >= System.DateTime.Now);

            return contractExists;
        }
    }
}
