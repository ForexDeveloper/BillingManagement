using Application.Query.Queries;
using Application.Query.QueryModels;
using Application.Query.ReadOnlyRepositoryContracts;
using Domain.Core.Enums;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repository.EfCore.ReadonlyRepositories
{
    public class TenantPlatformContractReadOnlyRepository : ITenantPlatformContractReadOnlyRepository
    {
        private readonly ReadonlyApplicationDbContext _readonlyApplicationDbContext;

        public TenantPlatformContractReadOnlyRepository(ReadonlyApplicationDbContext readonlyApplicationDbContext)
        {
            _readonlyApplicationDbContext = readonlyApplicationDbContext;
        }

        public async Task<TenantPlatformContractsQueryModel> GetListAsync(GetTenantPlatformContractsQuery request)
        {
            var query = _readonlyApplicationDbContext.TenantPlatformContracts
                .Include(x => x.Tenant)
                //.ThenInclude(x => x.ProjectManager)
                .AsQueryable();

            if (request.TenantId.HasValue)
            {
                query = query.Where(x => x.TenantId == request.TenantId);
            }

            if (!string.IsNullOrEmpty(request.ContractNumber))
            {
                query = query.Where(x => x.ContractNumber == request.ContractNumber);
            }

            if (request.StartDate.HasValue)
            {
                query = query.Where(x => x.StartDate == request.StartDate);
            }

            if (request.EndDate.HasValue)
            {
                query = query.Where(x => x.EndDate == request.EndDate);
            }

            if (request.FeeCalculationType.HasValue)
            {
                query = query.Where(x => x.FeeCalculationType == request.FeeCalculationType);
            }

            if (request.CommissionCalculationType.HasValue)
            {
                query = query.Where(x => x.CommissionCalculationType == request.CommissionCalculationType);
            }


            if (!string.IsNullOrWhiteSpace(request.SearchValue))
            {
                request.SearchValue = request.SearchValue.Trim();
                query = query.Where(x =>
                    x.ContractNumber.Contains(request.SearchValue) ||
                    x.Tenant.Title.Contains(request.SearchValue)
                );
            }

            if (!string.IsNullOrWhiteSpace(request.SortColumn))
            {
                query = request.SortColumn.ToLower() switch
                {
                    "tenantId" => request.SortDirection == Application.Query.Base.SortDirection.Ascending ?
                        query.OrderBy(x => x.TenantId) : query.OrderByDescending(x => x.TenantId),

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
                .Select(x => new TenantPlatformContractQueryModel
                {
                    Id = x.Id,
                    TenantId = x.TenantId,
                    TenantIName = x.Tenant.Title,
                    ContractNumber = x.ContractNumber,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    BrandName = x.Tenant.BrandName,
                    CreditProjectName = x.Tenant.CreditProjectName,
                    ProjectManagerName = x.Tenant.InternalProjectManagerName, // x.Tenant.ProjectManager != null ?
                                                                              //x.Tenant.ProjectManager.FullName : null,
                    Status = x.Status
                })
                .ToListAsync();


            return new TenantPlatformContractsQueryModel()
            {
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                Items = contracts,
                TotalCount = totalCounts
            };
        }

        public async Task<List<TenantPlatformContractSummaryQueryModel>> GetActiveContractsAsync(int tenantId)
        {
            var contracts = await _readonlyApplicationDbContext.TenantPlatformContracts
                .Where(x => x.TenantId == tenantId && x.Status == true && !x.IsDeleted && x.EndDate >= System.DateTime.Now)
                .Select(x => new TenantPlatformContractSummaryQueryModel
                {
                    Id = x.Id,
                    TenantId = x.TenantId,
                    TenantIpgSettingId = x.TenantIpgSettingId,
                    Status = x.Status
                })
                .ToListAsync();

            return contracts;
        }

        public async Task<List<ProviderQueryModel>> GetProvidersByTenantIdAsync(ProviderType? type, int tenantId)
        {
            var queryProvider = _readonlyApplicationDbContext.TenantPlatformContractProviders
                .Where(x => x.TenantPlatformContract.TenantId == tenantId && x.TenantPlatformContract.Status);

            if (type != null && type > 0)
            {
                queryProvider = queryProvider.Where(c => c.Provider.ProviderType == type);
            }
            var result = await queryProvider.Select(c => new ProviderQueryModel
            {
                Id = c.ProviderId,
                EnglishName = c.Provider.EnglishName,
                Name = c.Provider.Name,
                ProviderType = c.Provider.ProviderType
            }).ToListAsync();

            return result;
        }
    }
}
