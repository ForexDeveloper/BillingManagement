using Application.Query.Queries;
using Application.Query.QueryModels;
using Application.Query.ReadOnlyRepositoryContracts;
using Domain.Core.Entities.MerchantAggregate;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repository.EfCore.ReadonlyRepositories
{
    public class MerchantReadOnlyRepository : IMerchantReadOnlyRepository
    {
        private readonly ReadonlyApplicationDbContext _readonlyApplicationDbContext;
        public MerchantReadOnlyRepository(ReadonlyApplicationDbContext readonlyApplicationDbContext)
        {
            _readonlyApplicationDbContext = readonlyApplicationDbContext;
        }

        public async Task<List<MerchantListQueryModel>> GetListAsync(int tenatid)
        => await _readonlyApplicationDbContext.Merchants.Where(c => c.TenantId == tenatid).Select(c => new MerchantListQueryModel
        {
            Id = c.Id,
            Title = c.Title,
        }).ToListAsync();


        public async Task<MerchantBranch> GetBranchByMerchantIdAsync(int merchantId)
        {
            return await _readonlyApplicationDbContext.MerchantBranches
                .Include(b => b.Merchant)
                .FirstOrDefaultAsync(m => m.MerchantId == merchantId);
        }

        public async Task<MerchantBranch> GetBranchByIdAsync(int branchId)
        {
            return await _readonlyApplicationDbContext.MerchantBranches
                .Include(b => b.Merchant)
                .FirstOrDefaultAsync(m => m.Id == branchId);
        }

        public async Task<List<MerchantCategoryQueryModel>> GetCategoryByTenantIdAsync(int tenantId)
        => await _readonlyApplicationDbContext.MerchantCategories
       .Where(c => c.Merchant.TenantId == tenantId)
       .Select(m => new MerchantCategoryQueryModel
       {
           Title = m.Category.Title,
           Id = m.CategoryId,
       }).Distinct().ToListAsync();

        public async Task<List<int>> GetMerchantByFilterAsync(GetActiveWalletMerchantListQuery query)
        {
            var merchantQuery = _readonlyApplicationDbContext.Merchants
                        .Where(c => c.TenantId == query.TenantId);

            if (query.SaleType != null)
            {
                merchantQuery = merchantQuery.Where(c => c.SaleType == query.SaleType);
            }
            if (query.Categories != null && query.Categories.Any())
            {
                merchantQuery = merchantQuery.Where(c => c.MerchantCategories.Any(x => query.Categories.Contains(x.Category.ParentId.Value) || query.Categories.Contains(x.CategoryId)));
            }
            if (!string.IsNullOrEmpty(query.SearchValue))
            {
                merchantQuery = merchantQuery.Where(c => c.Title.Contains(query.SearchValue));
            }
            var result = await merchantQuery.Select(m => m.Id).ToListAsync();
            return result;
        }

        public async Task<MerchantBranch> GetMerchantBranchByTerminalIdAsync(long terminalId)
        {
            return await _readonlyApplicationDbContext.MerchantBranches
                .Include(b => b.Merchant)
                .FirstOrDefaultAsync(m => m.TerminalId == terminalId);
        }

        public async Task<List<MerchantBranchSummeryQueryModel>> GetMerchantBranchesByTenantIdAsync(int merchantId, int? tenantId)
        {
            return await _readonlyApplicationDbContext.MerchantBranches
               .Where(m => (!tenantId.HasValue || m.Merchant.TenantId == tenantId) && m.MerchantId == merchantId)
               .Select(c => new MerchantBranchSummeryQueryModel
               {
                   Id = c.Id,
                   Title = c.Title
               }).ToListAsync();
        }
    }
}
