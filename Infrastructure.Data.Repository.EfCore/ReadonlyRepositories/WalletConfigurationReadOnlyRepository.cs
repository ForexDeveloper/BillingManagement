using Application.Query.Queries;
using Application.Query.QueryModels;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repository.EfCore.Repositories
{
    public class WalletConfigurationReadOnlyRepository : IWalletConfigurationReadOnlyRepository
    {
        private readonly ReadonlyApplicationDbContext _readonlyApplicationDbContext;

        public WalletConfigurationReadOnlyRepository(ReadonlyApplicationDbContext readonlyApplicationDbContext)
        {
            _readonlyApplicationDbContext = readonlyApplicationDbContext;
        }

        public async Task<WalletConfigurationQueryModel> GetAsync(int id, int? tenantId = null)
        {
            var dataModel = await _readonlyApplicationDbContext.WalletConfigurations
                           .Include(c => c.Tenant)
                           .Include(c => c.ProjectManager)
                           .Include(c => c.CurrencyType)
                           .FirstOrDefaultAsync(x => x.Id == id && (!tenantId.HasValue || x.TenantId == tenantId));
            if (dataModel == null)
                return null;
            return new WalletConfigurationQueryModel(dataModel);
        }

        public async Task<GetWalletConfigurationQueryModel> GetListAsync(GetAllWalletConfigurationQuery request)
        {
            var walletConfigurationQuery = _readonlyApplicationDbContext.WalletConfigurations.AsQueryable();
            
            if (request.TenantId > 0)
            {
                walletConfigurationQuery = walletConfigurationQuery.Where(c => c.TenantId == request.TenantId);
            }
            if (!string.IsNullOrWhiteSpace(request.SearchValue))
            {
                request.SearchValue = request.SearchValue.Trim();
                walletConfigurationQuery = walletConfigurationQuery
                    .Where(x => x.Title.Contains(request.SearchValue) ||
                        x.ProjectManager.FullName.Contains(request.SearchValue) ||
                        x.Tenant.Title.Contains(request.SearchValue));
            }
            var totalCounts = await walletConfigurationQuery.CountAsync();

            walletConfigurationQuery = request.SortColumn?.ToLower() switch
            {
                "wallettypeid" => request.SortDirection == Application.Query.Base.SortDirection.Ascending ?
                                            walletConfigurationQuery.OrderBy(x => x.WalletTypeId) : walletConfigurationQuery.OrderByDescending(x => x.WalletTypeId),

                "maxwallet" => request.SortDirection == Application.Query.Base.SortDirection.Ascending ?
                                            walletConfigurationQuery.OrderBy(x => x.MaxWallet) : walletConfigurationQuery.OrderByDescending(x => x.MaxWallet),


                _ => walletConfigurationQuery.OrderByDescending(x => x.Id),
            };

            var result = await walletConfigurationQuery
            .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new WalletConfigurationsQueryModel()
                {
                    Id = x.Id,
                    Title = x.Title,
                    WalletTypeId = x.WalletTypeId,
                    MaxWallet = x.MaxWallet,
                    TenantId = x.TenantId,
                    ProjectManagerFullName = x.ProjectManager.FullName,
                    TenantTitle = x.Tenant.Title,
                    ProjectManagerId = x.ProjectManagerId,
                    PlanCount = x.Plans.Count
                }).ToListAsync();


            return new GetWalletConfigurationQueryModel()
            {
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                Items = result,
                TotalCount = totalCounts
            };
        }

        public async Task<List<GetWalletConfigurationWithOutCashWalletQueryModel>> GetListWithOutCashWalletAsync(int? tenantId)
        => await _readonlyApplicationDbContext.WalletConfigurations
                 .Where(c => c.WalletTypeId != Domain.Core.Enums.WalletType.Cash && (!tenantId.HasValue || c.TenantId == tenantId))
                  .Select(c => new GetWalletConfigurationWithOutCashWalletQueryModel
                  {
                      Id = c.Id,
                      Title = c.Title,
                  }).OrderByDescending(x => x.Id).ToListAsync();
    }

}
