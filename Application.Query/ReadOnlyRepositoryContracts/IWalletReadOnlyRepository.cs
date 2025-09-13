using Application.Query.Queries;
using Application.Query.QueryModels;
using Application.Service.Dtos.Wallet;
using Domain.Core.Enums;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.ReadOnlyRepositoryContracts;

public interface IWalletReadOnlyRepository
{
    Task<int> CountActiveWalletMerchantAsync(int customerId);
    Task<List<int>> GetActiveWalletMerchantListAsync(int customerId);
    Task<List<WalletQueryModel>> GetActiveWalletMerchantWithSaleTypeAsync(int customerId);
    Task<List<GetActiveWalletsListQueryModel>> GetActiveWalletsAsync(int customerId);
    Task<List<GetWalletsListQueryModel>> GetWalletsAsync(int customerId, int? tenantId);
    Task<WalletsQueryModel> GetWalletsAsync(GetWalletsPaginatedListQuery query);
    Task<List<GetActiveWalletCategoryQueryModel>> GetActiveWalletCategoryListAsync(int? walletId, int customerId);
    Task<List<int>> GetActiveWalletMerchantByFilterAsync(GetActiveWalletMerchantListQuery query);
    Task<List<GetWalletByMerchantIdQueryModel>> GetWalletByMerchantIdAsync(int customerId, int merchantId, WalletStatus? walletStatus = null);
    Task<List<GetSuperAppWalletQueryModel>> GetWalletByMerchantIdAsync(int customerId, int merchantId, int? tenantId);
    Task<decimal> CalculatePrePaymentAmount(int walletId, decimal amount);
    Task<GetWalletPrePaymentDetails> GetWalletPrePaymentDetails(int walletId, decimal amount);
    Task<GetCustomerWalletTransactionsQueryModel> GetCustomerWalletTransactionsAsync(GetCustomerWalletTransactionQuery query, CancellationToken cancellationToken);
    Task<GetCustomerWalletTransactionDetailQueryModel> GetCustomerWalletTransactionDetailAsync(GetCustomerWalletTransactionDetailQuery query, CancellationToken cancellationToken);
    Task<GetCustomerWalletInstallmentsInfoQueryModel> GetCustomerWalletInstallmentsInfoAsync(int customerId, int walletId, int? tenantId, CancellationToken cancellationToken);
    Task<GetWalletCurrentStateQueryModel> GetWalletAsync(int customerId, int walletId);
    Task<CutomerWalletDetailsQueryModel> GetCustomerWalletDetailsAsync(int? tenantId, int customerId, int walletId);
    Task<CashWalletQueryModel> GetCashWalletByCustomerIdAsync(int customerId, int? tenantId);
    Task<int?> GetWalletContractIdByWalletIdAsync(int tenantId, int walletId);
    Task<GetWalletQueryModel> GetWalletByCustomerIdAsync(int walletId, int customerId);
    Task<bool> HasCashWalletByIdAsync(List<int> keys, int tenantId);
    Task<bool> HasCashWalletByIdAsync(int customerId, int tenantId);
    Task<decimal> GetCustomerWalletBalance(int tenantId, int customerId, int walletId, CancellationToken cancellationToken);
}
