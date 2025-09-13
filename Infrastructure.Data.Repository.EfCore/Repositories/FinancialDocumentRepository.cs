using Domain.Core.Entities.FinancialDocumentAggregate;
using Domain.Core.Enums;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repository.EfCore.Repositories;

public class FinancialDocumentRepository : IFinancialDocumentRepository
{
    private readonly ApplicationDbContext _applicationDbContext;

    public FinancialDocumentRepository(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    public async Task AddAsync(FinancialDocument financialDocument)
    {
        await _applicationDbContext.FinancialDocuments.AddAsync(financialDocument);
    }

    public async Task<FinancialDocument> GetByIdAsync(long id)
    {
        return await _applicationDbContext.FinancialDocuments
            .Include(x => x.FinancialDocumentPayments).FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<FinancialDocument>> GetRefundsByParentIdAsync(long parentId)
    {
        return await _applicationDbContext.FinancialDocuments
            .Include(x => x.FinancialDocumentPayments).Where(p => p.ParentId == parentId && p.Type == FinancialDocumentType.Refund).ToListAsync();
    }

    public async Task<FinancialDocument> GetAsync(long paymentId)
    {
        return await _applicationDbContext.FinancialDocuments
            .Include(x => x.FinancialDocumentPayments).FirstOrDefaultAsync(p => p.PaymentId == paymentId);
    }

    public void Update(FinancialDocument financialDocument)
    {
        _applicationDbContext.FinancialDocuments.Update(financialDocument);
    }

    public void UpdateRange(List<FinancialDocument> financialDocuments)
    {
        _applicationDbContext.FinancialDocuments.UpdateRange(financialDocuments);
    }

    public async Task<List<FinancialDocument>> GetAllAsync()
    {
        return await _applicationDbContext.FinancialDocuments.Include(x => x.FinancialDocumentPayments).ToListAsync();
    }

    public async Task<bool> IsTenantPlatformContractUsedInTransaction(int tenantPlatformContractId)
    {
        return await _applicationDbContext.FinancialDocuments
            .AnyAsync(x => x.TenantPlatformContractId == tenantPlatformContractId);
    }

    public async Task<List<int>> GetTenantPlatformContractIdsHasTransaction(List<int> tenantPlatformContractIds)
    {
        var result = await _applicationDbContext.FinancialDocuments
            .Where(x => tenantPlatformContractIds.Contains(x.TenantPlatformContractId.Value))
            .Select(x => x.TenantPlatformContractId.Value)
            .ToListAsync();

        return result;
    }

    public async Task<bool> IsTenantMerchantContractUsedInTransaction(int tenantMerchantContractId)
    {
        return await _applicationDbContext.FinancialDocuments
            .AnyAsync(x => x.TenantMerchantContractId == tenantMerchantContractId);
    }

    public async Task<List<int>> GetTenantMerchantContractIdsHasTransaction(List<int> tenantMerchantContractIds)
    {
        var result = await _applicationDbContext.FinancialDocuments
            .Where(x => tenantMerchantContractIds.Contains(x.TenantMerchantContractId.Value))
            .Select(x => x.TenantMerchantContractId.Value)
            .ToListAsync();

        return result;
    }

    public async Task<bool> IsWalletContractUsedInTransaction(int walletContractId)
    {
        return await _applicationDbContext.FinancialDocumentPayments
            .AnyAsync(x => x.WalletContractId == walletContractId);
    }

    public async Task<List<int>> GetWalletContractIdsHasTransaction(List<int> walletContractIds)
    {
        var result = await _applicationDbContext.FinancialDocumentPayments
            .Where(x => walletContractIds.Contains(x.WalletContractId.Value))
            .Select(x => x.WalletContractId.Value)
            .ToListAsync();

        return result;
    }
}
