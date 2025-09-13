using Application.Query.Queries;
using Application.Query.QueryModels;
using Application.Query.ReadOnlyRepositoryContracts;
using Domain.Core.Entities.TenantMerchantContractAggregate;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repository.EfCore.ReadonlyRepositories
{
    public class TenantMerchantContractReadOnlyRepository : ITenantMerchantContractReadOnlyRepository
    {
        private readonly ReadonlyApplicationDbContext _readonlyApplicationDbContext;

        public TenantMerchantContractReadOnlyRepository(ReadonlyApplicationDbContext readonlyApplicationDbContext)
        {
            _readonlyApplicationDbContext = readonlyApplicationDbContext;
        }

        public async Task<TenantMerchantContract> GetByIdAsync(int id, int? tenantId)
        {
            var contract = await _readonlyApplicationDbContext.TenantMerchantContracts
                .Include(x => x.Tenant)
                .Include(x => x.Merchant)
                .FirstOrDefaultAsync(x => x.Id == id && (!tenantId.HasValue || x.TenantId == tenantId));

            return contract;
        }

        public async Task<TenantMerchantContractsQueryModel> GetListAsync(GetTenantMerchantContractsQuery request)
        {
            var query = _readonlyApplicationDbContext.TenantMerchantContracts
                .Include(x => x.Tenant)
                .Include(x => x.Merchant)
                .AsQueryable();

            if (request.TenantId.HasValue)
            {
                query = query.Where(x => x.TenantId == request.TenantId);
            }

            if (request.MerchantId.HasValue)
            {
                query = query.Where(x => x.MerchantId == request.MerchantId);
            }

            if (request.GuaranteeType.HasValue)
            {
                query = query.Where(x => x.GuaranteeType == request.GuaranteeType);
            }

            if (request.SettlementType.HasValue)
            {
                query = query.Where(x => x.SettlementType == request.SettlementType);
            }

            if (request.PaymentMethodType.HasValue)
            {
                query = query.Where(x => x.PaymentMethodType == request.PaymentMethodType);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchValue))
            {
                request.SearchValue = request.SearchValue.Trim();
                query = query.Where(x =>
                    x.ContractNumber.Contains(request.SearchValue) ||
                    x.Tenant.Title.Contains(request.SearchValue) ||
                    x.Merchant.Title.Contains(request.SearchValue)
                );
            }

            if (!string.IsNullOrWhiteSpace(request.SortColumn))
            {
                query = request.SortColumn.ToLower() switch
                {
                    "contractNumber" => request.SortDirection == Application.Query.Base.SortDirection.Ascending ?
                        query.OrderBy(x => x.ContractNumber) : query.OrderByDescending(x => x.ContractNumber),
                    "guaranteeType" => request.SortDirection == Application.Query.Base.SortDirection.Ascending ?
                        query.OrderBy(x => x.GuaranteeType) : query.OrderByDescending(x => x.GuaranteeType),
                    "TenantTitle" => request.SortDirection == Application.Query.Base.SortDirection.Ascending ?
                        query.OrderBy(x => x.Tenant.Title) : query.OrderByDescending(x => x.Tenant.Title),
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
                .Select(x => new TenantMerchantContractQueryModel
                {
                    Id = x.Id,
                    TenantId = x.TenantId,
                    TenantName = x.Tenant.Title,
                    MerchantId = x.MerchantId,
                    MerchantName = x.Merchant.Title,
                    ContractNumber = x.ContractNumber,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    CreatedDateTime = x.CreatedDateTime,
                    Status = x.Status
                })
                .ToListAsync();


            return new TenantMerchantContractsQueryModel()
            {
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                Items = contracts,
                TotalCount = totalCounts
            };
        }

        public async Task<bool> GetActiveContractAsync(int tenantId, int merchantId)
        {
            var contractExists = await _readonlyApplicationDbContext.TenantMerchantContracts
                .AnyAsync(x => x.TenantId == tenantId && x.MerchantId == merchantId && x.Status == true && !x.IsDeleted && x.EndDate >= System.DateTime.Now);

            return contractExists;
        }
    }
}
