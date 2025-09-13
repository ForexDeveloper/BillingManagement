using Application.Query.Queries;
using Application.Query.QueryModels;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Service.Dtos.Wallet;
using Domain.Core.Enums;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repository.EfCore.ReadonlyRepositories;

public class WalletReadOnlyRepository : IWalletReadOnlyRepository
{
    private readonly ReadonlyApplicationDbContext _readonlyApplicationDbContext;

    public WalletReadOnlyRepository(ReadonlyApplicationDbContext readonlyApplicationDbContext)
    {
        _readonlyApplicationDbContext = readonlyApplicationDbContext;
    }
    public async Task<GetCustomerWalletTransactionsQueryModel> GetCustomerWalletTransactionsAsync(GetCustomerWalletTransactionQuery query, CancellationToken cancellationToken)
    {
        var dbQuery = _readonlyApplicationDbContext.Wallets
            .Where(w => w.Id == query.WalletId)
            .Join(_readonlyApplicationDbContext.Accounts,
                w => w.AccountId,
                a => a.Id,
                (w, a) => new { Wallet = w, Account = a })
            .Where(wa => wa.Account.BusinessIdentityId == query.CustomerId)
            .SelectMany(wa =>
                    _readonlyApplicationDbContext.Transactions
                        .Where(t => (t.FromAccountId == wa.Account.Id || t.ToAccountId == wa.Account.Id) &&
                                    (query.Types.Count == 0 || query.Types.Contains(t.Type))
                        ),
                (wa, t) => new
                {
                    Wallet = wa.Wallet,
                    Account = wa.Account,
                    Transaction = t
                });



        var totalCounts = await dbQuery.CountAsync(cancellationToken);

        var result = await dbQuery
          .Select(p => new WalletTransactionsQueryModel
          {
              Id = p.Transaction.Id,
              Type = p.Transaction.Type,
              Description = p.Transaction.Description,
              Amount = p.Transaction.Amount,
              TransactionTime = p.Transaction.CreatedDateTime
          })
          .OrderByDescending(x => x.TransactionTime)
          .Skip((query.PageIndex - 1) * query.PageSize)
          .Take(query.PageSize)
          .ToListAsync(cancellationToken);


        return new GetCustomerWalletTransactionsQueryModel
        {
            Items = result,
            PageIndex = query.PageIndex,
            PageSize = query.PageSize,
            TotalCount = totalCounts
        };
    }

    public async Task<GetCustomerWalletTransactionDetailQueryModel> GetCustomerWalletTransactionDetailAsync(GetCustomerWalletTransactionDetailQuery query, CancellationToken cancellationToken)
    {
        var transactionResult = from transaction in _readonlyApplicationDbContext.Transactions

                                    // LEFT JOIN fromWallet
                                join fromWallet in _readonlyApplicationDbContext.Wallets.Include(x => x.Plan)
                                on transaction.FromAccountId equals fromWallet.AccountId into fromWallets
                                from fromWallet in fromWallets.DefaultIfEmpty()

                                    // LEFT JOIN toWallet
                                join toWallet in _readonlyApplicationDbContext.Wallets.Include(x => x.Plan)
                                on transaction.ToAccountId equals toWallet.AccountId into toWallets
                                from toWallet in toWallets.DefaultIfEmpty()

                                where transaction.Id == query.TransactionId

                                select new GetCustomerWalletTransactionDetailQueryModel
                                {
                                    TransactionId = transaction.Id,
                                    Amount = transaction.Amount,
                                    TransactionTime = transaction.CreatedDateTime,
                                    Type = transaction.Type,
                                    Description = transaction.Description,
                                    FromWalletName = fromWallet != null ? fromWallet.Plan.Title : null,
                                    ToWalletName = toWallet != null ? toWallet.Plan.Title : null
                                };

        var result = await transactionResult.FirstOrDefaultAsync();
        //var transaction1 = await _readonlyApplicationDbContext
        //      .Transactions
        //      .Where(p => p.Id == query.TransactionId && (!query.TenantId.HasValue || p.TenantId == query.TenantId))
        //      .FirstOrDefaultAsync(cancellationToken);

        await GetWalletTransactionDetailAsync(query.WalletId, result, cancellationToken);

        return result;
    }

    public async Task<GetCustomerWalletInstallmentsInfoQueryModel> GetCustomerWalletInstallmentsInfoAsync(int customerId, int walletId, int? tenantId, CancellationToken cancellationToken)
    {
        var wallet = _readonlyApplicationDbContext.Wallets
          .Where(x => x.Id == walletId && x.BusinessIdentityId == customerId).FirstOrDefault();

        var installments = await _readonlyApplicationDbContext.Installments
            .Where(x => x.FromAccount.BusinessIdentityId == customerId && x.FromAccountId == wallet.AccountId).ToListAsync();

        var result = new GetCustomerWalletInstallmentsInfoQueryModel
        {
            FirstInstallmentDueDate = installments.Min(g => g.DueDate),
            LastInstallmentDueDate = installments.Max(g => g.DueDate),
            PaiedInstallmentCount = installments.Where(x => x.State == InstallmentState.CompletePaid && x.ParentId == null).Count(),
            PaiedAmount = installments.Sum(x => x.PaidAmount),
            RemainingAmount = installments.Sum(x => x.Amount) - installments.Sum(x => x.PaidAmount),
            CustomerWalletInstallments = installments.Where(x => x.ParentId == null).Select(x => new CustomerWalletInstallment
            {
                DueDate = x.DueDate,
                InstallmentIdentity = x.Id.ToString(),
                InstallmentState = x.State
            }).ToList()
        };

        return result;
    }

    public async Task<int> CountActiveWalletMerchantAsync(int customerId)
    {
        var result = await _readonlyApplicationDbContext.Wallets
            .Where(c => c.Status == Domain.Core.Enums.WalletStatus.Active && c.BusinessIdentityId == customerId)
            .SelectMany(c => c.Plan.PlanClosedloops)
            .SelectMany(cl => cl.ClosedLoop.ClosedloopMerchants)
            .Select(c => c.MerchantId)
            .Distinct()
            .CountAsync();
        return result;
    }

    public async Task<List<GetActiveWalletsListQueryModel>> GetActiveWalletsAsync(int customerId)
    {
        return await _readonlyApplicationDbContext.Wallets
                               .Where(c => c.Status == WalletStatus.Active &&
                               c.BusinessIdentityId == customerId).Select(c => new GetActiveWalletsListQueryModel
                               {
                                   WalletType = c.Plan.WalletConfiguration.WalletTypeId,
                                   Id = c.Id,
                                   Title = c.Plan.Title,
                                   Balance = c.Account.Balance,
                               }).ToListAsync();
    }

    public async Task<List<GetWalletsListQueryModel>> GetWalletsAsync(int customerId, int? tenantId)
    {
        return await _readonlyApplicationDbContext.Wallets
            .Where(c => c.BusinessIdentityId == customerId && (!tenantId.HasValue || c.TenantId == tenantId))
            .Select(c => new GetWalletsListQueryModel
            {
                TenantId = c.TenantId,
                PlanId = c.PlanId,
                WalletType = c.Plan.WalletConfiguration.WalletTypeId,
                Id = c.Id,
                Title = c.Plan.Title,
                Balance = c.Account.Balance,
                OrganizationTitle = c.WalletContract.Organization.Title,
                WalletStatus = c.Status,
                IsDefault = c.IsDefault
            }).ToListAsync();
    }

    public async Task<WalletsQueryModel> GetWalletsAsync(GetWalletsPaginatedListQuery query)
    {
        var walletQuery = _readonlyApplicationDbContext.Wallets
            .Where(c => c.BusinessIdentityId == query.CustomerId && (!query.TenantId.HasValue || c.TenantId == query.TenantId));
        var totalCounts = await walletQuery.CountAsync();

        var wallets = await walletQuery
             .Select(c => new GetWalletsListQueryModel
             {
                 TenantId = c.TenantId,
                 PlanId = c.PlanId,
                 WalletType = c.Plan.WalletConfiguration.WalletTypeId,
                 Id = c.Id,
                 Title = c.Plan.Title,
                 Balance = c.Account.Balance,
                 OrganizationTitle = c.WalletContract.Organization.Title,
                 WalletStatus = c.Status,
                 IsDefault = c.IsDefault
             })
            .OrderBy(x => x.WalletType)
            .Skip((query.PageIndex - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new WalletsQueryModel
        {
            Items = wallets,
            PageIndex = query.PageIndex,
            PageSize = query.PageSize,
            TotalCount = totalCounts
        };
    }

    public async Task<List<GetActiveWalletCategoryQueryModel>> GetActiveWalletCategoryListAsync(int? walletId, int customerId)
     => await _readonlyApplicationDbContext.Wallets
            .Where(c => c.Status == WalletStatus.Active && c.BusinessIdentityId == customerId
            && (!walletId.HasValue || c.Id == walletId))
            .SelectMany(c => c.Plan.PlanClosedloops)
            .SelectMany(cl => cl.ClosedLoop.ClosedloopMerchants)
            .SelectMany(m => m.Merchant.MerchantCategories)
            .Select(m => new GetActiveWalletCategoryQueryModel
            {
                Title = m.Category.Title,
                Id = m.CategoryId,
            }).Distinct().ToListAsync();

    public async Task<List<int>> GetActiveWalletMerchantByFilterAsync(GetActiveWalletMerchantListQuery query)
    {
        var walletQuery = _readonlyApplicationDbContext.Wallets
                         .Where(c => c.Status == Domain.Core.Enums.WalletStatus.Active
                          && c.BusinessIdentityId == query.CustomerId
                          && (query.Wallets == null || !query.Wallets.Any() || query.Wallets.Contains(c.Id)))
                         .SelectMany(c => c.Plan.PlanClosedloops)
                         .SelectMany(cl => cl.ClosedLoop.ClosedloopMerchants);

        if (query.SaleType != null)
        {
            walletQuery = walletQuery.Where(c => c.Merchant.SaleType == query.SaleType);
        }
        if (query.Categories != null && query.Categories.Any())
        {
            walletQuery = walletQuery.Where(c => c.Merchant.MerchantCategories.Any(x => query.Categories.Contains(x.Category.ParentId.Value) || query.Categories.Contains(x.CategoryId)));
        }
        if (!string.IsNullOrEmpty(query.SearchValue))
        {
            walletQuery = walletQuery.Where(c => c.Merchant.Title.Contains(query.SearchValue));
        }
        var result = await walletQuery.Select(m => m.MerchantId).ToListAsync();
        return result;
    }

    public async Task<List<int>> GetActiveWalletMerchantListAsync(int customerId)
    {
        var result = await _readonlyApplicationDbContext.Wallets
            .Where(c => c.Status == Domain.Core.Enums.WalletStatus.Active && c.BusinessIdentityId == customerId)
            .SelectMany(c => c.Plan.PlanClosedloops)
            .SelectMany(cl => cl.ClosedLoop.ClosedloopMerchants)
            .Select(m => m.MerchantId).ToListAsync();
        return result;
    }

    public async Task<List<WalletQueryModel>> GetActiveWalletMerchantWithSaleTypeAsync(int customerId)
    {
        var result = await _readonlyApplicationDbContext.Wallets
              .Where(c => c.Status == Domain.Core.Enums.WalletStatus.Active && c.BusinessIdentityId == customerId)
              .SelectMany(c => c.Plan.PlanClosedloops)
              .SelectMany(cl => cl.ClosedLoop.ClosedloopMerchants)
              .Select(m => new WalletQueryModel
              {
                  MechantId = m.MerchantId,
                  SaleType = m.Merchant.SaleType
              }).Distinct().ToListAsync();
        return result;
    }

    public async Task<List<GetWalletByMerchantIdQueryModel>> GetWalletByMerchantIdAsync(int customerId, int merchantId, WalletStatus? walletStatus = null)
    {
        var merchant = await _readonlyApplicationDbContext.Merchants
            .Include(x => x.MerchantCategories)
            .FirstOrDefaultAsync(x => x.Id == merchantId);

        var merchantCategoryIds = merchant.MerchantCategories.Select(x => x.CategoryId).ToList();

        var result = await _readonlyApplicationDbContext.LoanWallets
        .Where(c => c.BusinessIdentityId == customerId && ((!walletStatus.HasValue || c.Status == walletStatus)) &&
                    (c.Plan.PlanClosedloops.Any(p =>
                        p.ClosedLoop.ClosedloopMerchants.Any(m => m.MerchantId == merchantId)
                     ) ||
                     c.Plan.PlanClosedloops.Any(p =>
                        p.ClosedLoop.ClosedloopCategories
                            .Any(m => merchantCategoryIds.Contains(m.CategoryId))
                     )
                    )
                    )
        .Select(w => new GetWalletByMerchantIdQueryModel
        {
            NumberOfInstallment = w.NumberOfInstallment,
            Balance = w.Account.Balance,
            Id = w.Id,
            PlanId = w.PlanId,
            Title = w.Plan.Title,
            WalletType = w.Plan.WalletConfiguration.WalletTypeId,
            WalletStatus = w.Status,
            IsDefault = w.IsDefault,
            PlanDetails = w.Plan.PlanDetails.Select(c => new GetPlanDetailQueryModel
            {
                Installments = c.PlanDetailInstallments.Select(v => v.NumberOfInstallment),
                PrepaymentMaxAmount = c.PrepaymentMaxAmount,
                PrepaymentMinAmount = c.PrepaymentMinAmount,
                PrepaymentPercent = c.PrepaymentPercent
            })
        }).ToListAsync();

        return result;
    }

    public async Task<List<GetSuperAppWalletQueryModel>> GetWalletByMerchantIdAsync(int customerId, int merchantId, int? tenantId)
    {
        var categoryIds = _readonlyApplicationDbContext.Merchants.Where(p => p.Id == merchantId)
            .SelectMany(p => p.MerchantCategories.Select(q => q.CategoryId));

        var wallets = await _readonlyApplicationDbContext.LoanWallets
            .Where(p => p.BusinessIdentityId == customerId && (!tenantId.HasValue || p.TenantId == tenantId) &&
                        (p.Plan.PlanClosedloops.Any(q =>
                             q.ClosedLoop.ClosedloopMerchants.Any(r => r.MerchantId == merchantId)) ||
                         p.Plan.PlanClosedloops.Any(q =>
                             q.ClosedLoop.ClosedloopCategories.Any(r => categoryIds.Contains(r.CategoryId)))))
            .Select(p => new GetSuperAppWalletQueryModel()
            {
                Id = p.Id,
                PlanId = p.PlanId,
                Title = p.Plan.Title,
                WalletStatus = p.Status,
                IsDefault = p.IsDefault,
                Balance = p.Account.Balance,
                WalletType = p.Plan.WalletConfiguration.WalletTypeId,
                OrganizationTitle = p.WalletContract.Organization.Title
            }).ToListAsync();

        return wallets;
    }

    public async Task<decimal> CalculatePrePaymentAmount(int walletId, decimal amount)
    {
        var calculatedPrePaymentAmount = 0m;

        var result = await (from lw in _readonlyApplicationDbContext.LoanWallets
                            join w in _readonlyApplicationDbContext.Wallets on lw.Id equals w.Id
                            join p in _readonlyApplicationDbContext.Plans on w.PlanId equals p.Id
                            join pd in _readonlyApplicationDbContext.PlanDetails on p.Id equals pd.PlanId
                            join pdi in _readonlyApplicationDbContext.PlanDetailInstallments on pd.Id equals pdi.PlanDetailId
                            where w.Id == walletId && lw.NumberOfInstallment == pdi.NumberOfInstallment
                            select new PrePaymentAmountDto
                            {
                                PrepaymentPercent = pd.PrepaymentPercent,
                                PrepaymentMinAmount = pd.PrepaymentMinAmount,
                                PrepaymentMaxAmount = pd.PrepaymentMaxAmount
                            }).FirstOrDefaultAsync();

        if (result == null)
        {
            return 0;
        }

        if (result.PrepaymentPercent is null or 0)
        {
            calculatedPrePaymentAmount = 0;
        }
        else
        {
            calculatedPrePaymentAmount = (decimal)(result.PrepaymentPercent * amount * .01m);

            if (result.PrepaymentMinAmount.HasValue && calculatedPrePaymentAmount < result.PrepaymentMinAmount)
            {
                calculatedPrePaymentAmount = result.PrepaymentMinAmount.Value;
            }
            else if (result.PrepaymentMaxAmount.HasValue && calculatedPrePaymentAmount > result.PrepaymentMaxAmount)
            {
                calculatedPrePaymentAmount = result.PrepaymentMaxAmount.Value;
            }
        }

        return calculatedPrePaymentAmount;
    }

    public decimal CalculatePrePaymentAmount(PrePaymentAmountDto request, decimal amount)
    {
        var calculatedPrePaymentAmount = 0m;

        if (request.PrepaymentPercent is null or 0)
        {
            calculatedPrePaymentAmount = 0;
        }
        else
        {
            calculatedPrePaymentAmount = (decimal)(request.PrepaymentPercent * amount * .01m);

            if (request.PrepaymentMinAmount.HasValue && calculatedPrePaymentAmount < request.PrepaymentMinAmount)
            {
                calculatedPrePaymentAmount = request.PrepaymentMinAmount.Value;
            }
            else if (request.PrepaymentMaxAmount.HasValue && calculatedPrePaymentAmount > request.PrepaymentMaxAmount)
            {
                calculatedPrePaymentAmount = request.PrepaymentMaxAmount.Value;
            }
        }

        return calculatedPrePaymentAmount;
    }

    public async Task<GetWalletPrePaymentDetails> GetWalletPrePaymentDetails(int walletId, decimal amount)
    {
        var calculatedPrePaymentAmount = 0m;

        var result = await (from lw in _readonlyApplicationDbContext.LoanWallets
                            join w in _readonlyApplicationDbContext.Wallets on lw.Id equals w.Id
                            join p in _readonlyApplicationDbContext.Plans on w.PlanId equals p.Id
                            join pd in _readonlyApplicationDbContext.PlanDetails on p.Id equals pd.PlanId
                            join pdi in _readonlyApplicationDbContext.PlanDetailInstallments on pd.Id equals pdi.PlanDetailId
                            where w.Id == walletId && lw.NumberOfInstallment == pdi.NumberOfInstallment
                            select new GetWalletPrePaymentDetails
                            {
                                PrepaymentPercent = pd.PrepaymentPercent,
                                PrepaymentMinAmount = pd.PrepaymentMinAmount,
                                PrepaymentMaxAmount = pd.PrepaymentMaxAmount,
                                CustomerId = lw.BusinessIdentityId,
                                Balance = w.Account.Balance,
                            }).FirstOrDefaultAsync();

        var calculatedPrePaymentAmountRequest = new PrePaymentAmountDto()
        {
            PrepaymentMaxAmount = result?.PrepaymentMaxAmount,
            PrepaymentMinAmount = result?.PrepaymentMinAmount,
            PrepaymentPercent = result?.PrepaymentPercent
        };

        calculatedPrePaymentAmount = CalculatePrePaymentAmount(calculatedPrePaymentAmountRequest, amount);

        if (result != null)
        {
            result.CalculatedPrepaymentAmount = calculatedPrePaymentAmount;
        }

        return result;
    }
    public async Task<CutomerWalletDetailsQueryModel> GetCustomerWalletDetailsAsync(int? tenantId, int customerId, int walletId)
    {

        var result = await _readonlyApplicationDbContext.Wallets
         .SelectMany(wallet => _readonlyApplicationDbContext.LoanWallets
          .Where(loanWallet => loanWallet.Id == wallet.Id).DefaultIfEmpty(),
          (wallet, loanWallet) => new { wallet, loanWallet })
          .Where(x => x.wallet.BusinessIdentityId == customerId &&
                  x.wallet.Id == walletId &&
                  (!tenantId.HasValue || x.wallet.TenantId == tenantId))
      .Select(x => new CutomerWalletDetailsQueryModel
      {
          Type = x.wallet.Plan.WalletConfiguration.WalletTypeId,
          Status = x.wallet.Status,
          CreateDateTime = x.wallet.CreatedDateTime,
          Balance = x.wallet.Account.Balance,
          InitialAmount = x.loanWallet.InitialAmount,
          InstallmentsCount = x.loanWallet.NumberOfInstallment,
          TermsAndConditions = x.wallet.Plan.TermsAndConditions,
          OrganizationName = x.wallet.WalletContract.Organization.Title
      }).FirstOrDefaultAsync();

        return result;
    }

    public async Task<int?> GetWalletContractIdByWalletIdAsync(int tenantId, int walletId)
    {
        var wallet = await _readonlyApplicationDbContext.Wallets
            .Include(x => x.WalletContract)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == walletId && x.WalletContract != null && x.WalletContract.Status == WalletContractStatus.Active && !x.WalletContract.IsDeleted && x.WalletContract.EndDate >= System.DateTime.Now);

        return wallet?.WalletContractId;
    }



    public async Task<CashWalletQueryModel> GetCashWalletByCustomerIdAsync(int customerId, int? tenantId)
    {
        var cashWallet = await _readonlyApplicationDbContext.CashWallets
            .Where(c => c.BusinessIdentityId == customerId)
            .Select(cw => new CashWalletQueryModel
            {
                Id = cw.Id,
                Balance = cw.Account.Balance,
                NonWithDrawableBalance = cw.Account.NonWithDrawableBalance,
                WithDrawableBalance = cw.Account.WithDrawableBalance,
                Status = cw.Account.Status,
                TermsAndConditions = cw.Plan.TermsAndConditions
            })
            .FirstOrDefaultAsync();

        return cashWallet;
    }

    public async Task<GetWalletCurrentStateQueryModel> GetWalletAsync(int customerId, int walletId)
    {

        var wallet = _readonlyApplicationDbContext.Wallets.Include(x => x.Plan)
            .ThenInclude(x => x.WalletConfiguration)
            .Where(x => x.Id == walletId
            && x.BusinessIdentityId == customerId).FirstOrDefault();

        var result = await _readonlyApplicationDbContext.Installments
                           .Where(c => c.FromAccountId == wallet.AccountId)
                           .Select(c => new GetWalletCurrentStateQueryModel
                           {
                               Balance = c.FromAccount.Balance,
                               Type = wallet.Plan.WalletConfiguration.WalletTypeId,
                               Title = wallet.Plan.Title,

                               HasOverdue = _readonlyApplicationDbContext.Installments
                               .Where(x => x.FromAccountId == wallet.AccountId).Count(c => c.State == InstallmentState.Overdue) > 0,

                               HasPending = _readonlyApplicationDbContext.Installments
                               .Where(x => x.FromAccountId == wallet.AccountId).Count(c => c.State == InstallmentState.Pending) > 0,

                               IsDefault = wallet.IsDefault,
                               Status = wallet.Status,
                               PlanId = wallet.PlanId,
                               AccountId = c.FromAccountId,
                               TermsAndConditions = wallet.Plan.TermsAndConditions
                           }).FirstOrDefaultAsync();


        return result;
    }

    public async Task<GetWalletQueryModel> GetWalletByCustomerIdAsync(int walletId, int customerId)
    {
        var result = await _readonlyApplicationDbContext.Wallets.Where(c => c.Id == walletId && c.BusinessIdentityId == customerId).Select(c => new GetWalletQueryModel
        {
            Id = c.Id,
            WalletTypeId = c.Plan.WalletConfiguration.WalletTypeId
        }).FirstOrDefaultAsync();
        return result;
    }

    public async Task<bool> HasCashWalletByIdAsync(List<int> keys, int tenantId)
    => await _readonlyApplicationDbContext.Wallets.AnyAsync(c => keys.Contains(c.Id)
    && c.Plan.WalletConfiguration.WalletTypeId == WalletType.Cash);

    public async Task<bool> HasCashWalletByIdAsync(int customerId, int tenantId)
  => await _readonlyApplicationDbContext.Wallets.AnyAsync(c => c.BusinessIdentityId == customerId
  && c.Plan.WalletConfiguration.WalletTypeId == WalletType.Cash);

    public async Task<decimal> GetCustomerWalletBalance(int tenantId, int customerId, int walletId, CancellationToken cancellationToken)
    {
        var y = await _readonlyApplicationDbContext.LoanWallets
            .Include(x => x.Account)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.BusinessIdentityId == customerId && x.Id == walletId && x.AccountId == x.Account.Id, cancellationToken: cancellationToken);

        return y != null ? y.Account.Balance : 0;
    }

    #region private_methods
    private async Task GetWalletTransactionDetailAsync(int walletId, GetCustomerWalletTransactionDetailQueryModel result, CancellationToken cancellationToken)
    {
        switch (result.Type)
        {
            case TransactionType.Transfer:
            case TransactionType.Purchase:
                result.PurchaseTransactionDetail = await GetPurchaseTransactionDetail(result.TransactionId, cancellationToken);
                break;

            case TransactionType.Charge:
                result.WalletChargeTransactionDetail = await GetWalletChargeTransactionDetail(result.TransactionId, cancellationToken);
                break;

            case TransactionType.Payment:
                result.BillingTransactionDetail = await GetBaseTransactionDetail(result.TransactionId, cancellationToken);
                break;

            case TransactionType.Withdrawal:
                result.WithdrawalTransactionDetail = await GetBaseTransactionDetail(result.TransactionId, cancellationToken);
                break;

            case TransactionType.Reverse:
                result.ReverseTransactionDetail = await GetReverseTransactionDetail(result.TransactionId, cancellationToken);
                break;


            case TransactionType.Refund:
                result.RefundTransactionDetail = await GetReverseTransactionDetail(result.TransactionId, cancellationToken);
                break;

            case TransactionType.OperationalFee:
            case TransactionType.VerificationFee:
                result.OperationalFeeTransactionDetail = await GetOperationalFeeTransactionDetail(result.TransactionId, cancellationToken);

                break;

            default:
                break;

        }
    }

    private async Task<ReverseTransactionDetail> GetReverseTransactionDetail(long transactionId, CancellationToken cancellationToken)
    {
        return await _readonlyApplicationDbContext.FinancialDocumentPayments
             .Include(p => p.Wallet)
             .ThenInclude(p => p.Plan)
             .Where(c => c.TransactionId == transactionId)
             .Select(p => new ReverseTransactionDetail
             {
                 FinancialDocumentId = p.FinancialDocumentId,
                 WalletName = p.Wallet.Plan.Title,
             })
             .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<BaseTransactionDetail> GetBaseTransactionDetail(long transactionId, CancellationToken cancellationToken)
    {
        return await _readonlyApplicationDbContext.FinancialDocumentPayments
             .Include(p => p.Wallet)
             .ThenInclude(p => p.Plan)
             .Where(c => c.TransactionId == transactionId)
             .Select(p => new BaseTransactionDetail
             {
                 WalletName = p.Wallet.Plan.Title,
             })
             .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<WalletChargeTransactionDetail> GetWalletChargeTransactionDetail(long transactionId, CancellationToken cancellationToken)
    {
        return await _readonlyApplicationDbContext.FinancialDocumentPayments
             .Include(p => p.Wallet)
             .ThenInclude(p => p.Plan)
             .Where(c => c.TransactionId == transactionId)
             .Select(p => new WalletChargeTransactionDetail
             {
                 WalletName = p.Wallet.Plan.Title,
             })
             .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<OperationalFeeTransactionDetail> GetOperationalFeeTransactionDetail(long transactionId, CancellationToken cancellationToken)
    {
        return await _readonlyApplicationDbContext.FinancialDocumentPayments
             .Include(p => p.Wallet)
             .ThenInclude(p => p.Plan)
             .Where(c => c.TransactionId == transactionId)
             .Select(p => new OperationalFeeTransactionDetail
             {
                 WalletName = p.Wallet.Plan.Title,
                 TrackingCode = "" //TODO ask
             })
             .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<PurchaseTransactionDetail> GetPurchaseTransactionDetail(long transactionId, CancellationToken cancellationToken)
    {
        return await _readonlyApplicationDbContext.FinancialDocumentPayments
             .Include(p => p.Wallet)
             .ThenInclude(p => p.Plan)
             .Where(c => c.TransactionId == transactionId)
             .Select(p => new PurchaseTransactionDetail
             {
                 FinancialDocumentId = p.FinancialDocumentId,
                 WalletName = p.Wallet.Plan.Title,
             })
             .FirstOrDefaultAsync(cancellationToken);
    }
    #endregion
}