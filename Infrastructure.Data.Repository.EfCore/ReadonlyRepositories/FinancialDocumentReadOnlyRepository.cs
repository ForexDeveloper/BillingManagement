using Application.Query.Queries;
using Application.Query.QueryModels;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Tenants;
using Domain.Core.Entities.FinancialDocumentAggregate;
using Domain.Core.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repository.EfCore.ReadonlyRepositories;

public class FinancialDocumentReadOnlyRepository : IFinancialDocumentReadOnlyRepository
{
    private readonly ReadonlyApplicationDbContext _context;

    public FinancialDocumentReadOnlyRepository(ReadonlyApplicationDbContext readonlyApplicationDbContext)
    {
        _context = readonlyApplicationDbContext;
    }

    public async Task<CustomerFinancialDocumentDetailQueryModel> GetCustomerFinancialDocumentAsync(GetCustomerFinancialDocumentQuery query, CancellationToken cancellationToken)
    {
        var financialDocument = await _context.FinancialDocuments.Where(c =>
            (c.FromBusinessIdentityId == query.Id || c.ToBusinessIdentityId == query.Id)
            && (!query.TenantId.HasValue || c.TenantId == query.TenantId)
            && c.Id == query.FinancialDocumentId).FirstOrDefaultAsync(cancellationToken);

        if (financialDocument is null)
        {
            return null;
        }

        var result = new CustomerFinancialDocumentDetailQueryModel
        {
            Amount = financialDocument.Amount,
            Time = financialDocument.CreatedDateTime,
            Type = financialDocument.Type,
            State = financialDocument.State,
            PaymentGatewayType = financialDocument.PaymentGatewayType,
            Description = financialDocument.Description
        };

        await GetFinancialDocumentDetailAsync(financialDocument, result, cancellationToken);

        return result;
    }

    public async Task<CustomerFinancialDocumentsQueryModel> GetCustomerFinancialDocumentsAsync(GetCustomerFinancialDocumentsQuery query, CancellationToken cancellationToken)
    {
        var queryFinancialDocuments =
            _context.FinancialDocuments
                    .Where(c => (c.FromBusinessIdentityId == query.Id || c.ToBusinessIdentityId == query.Id) && (!query.TenantId.HasValue || c.TenantId == query.TenantId));


        if (query.Types.Count != 0)

            queryFinancialDocuments = queryFinancialDocuments.Where(p => query.Types.Contains(p.Type));

        if (query.FromDate.HasValue)

            queryFinancialDocuments = queryFinancialDocuments.Where(p => p.CreatedDateTime >= query.FromDate.Value);

        if (query.ToDate.HasValue)

            queryFinancialDocuments = queryFinancialDocuments.Where(p => p.CreatedDateTime <= query.ToDate.Value);

        var totalCounts = await queryFinancialDocuments.CountAsync(cancellationToken);

        var result = await queryFinancialDocuments
            .OrderByDescending(p => p.CreatedDateTime)
            .Skip((query.PageIndex - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => new CustomerFinancialDocumentQueryModel()
            {
                Id = x.Id,
                Amount = x.Amount,
                TransactionTime = x.CreatedDateTime,
                Type = x.Type,
                Description = x.Description

            }).ToListAsync(cancellationToken);

        return new CustomerFinancialDocumentsQueryModel
        {
            Items = result,
            PageIndex = query.PageIndex,
            PageSize = query.PageSize,
            TotalCount = totalCounts
        };


    }

    public async Task<GetPurchaseVm> GetPurchaseAsync(long financialDocumentId, CancellationToken cancellationToken)
    {
        var query = await _context.FinancialDocuments
             .Where(fd => fd.Type == FinancialDocumentType.Purchase && fd.Id == financialDocumentId)
             .SelectMany(fd => fd.FinancialDocumentPayments.DefaultIfEmpty(),
                 (fd, payment) => new { fd, payment })
             .GroupJoin(_context.Wallets,
                 x => x.payment != null ? x.payment.WalletId : null,
                 w => w.Id,
                 (x, wallets) => new { x.fd, x.payment, wallets })
             .SelectMany(x => x.wallets.DefaultIfEmpty(),
                 (x, wallet) => new { x.fd, x.payment, wallet })
             .Join(_context.Plans,
                 w => w.wallet.PlanId,
                 p => p.Id,
                 (w, p) => new { FinancialDocument = w.fd, FdPayment = w.payment, Plan = p, w.wallet })
             .Join(_context.Customers,
                 abcd => abcd.FinancialDocument.FromBusinessIdentityId,
                 c => c.Id,
                 (abcd, c) => new { abcd, customer = c })
             .Join(_context.Merchants,
                 abdef => abdef.abcd.FinancialDocument.ToBusinessIdentityId,
                 m => m.Id,
                 (abcde, m) => new { abcde.abcd.FinancialDocument, abcde.abcd.Plan, abcde.abcd.FdPayment, abcde.abcd.wallet, abcde.customer, merchant = m })
             .Join(_context.MerchantBranches,
                 abdefg => abdefg.merchant.Id,
                 mb => mb.MerchantId,
                 (abdefg, merchantBranch) => new GetPurchaseVm
                 {
                     Id = financialDocumentId,
                     CustomerFullName = abdefg.customer.FullName,
                     CustomerNationalCode = abdefg.customer.NationalId,
                     CustomerMobileNumber = abdefg.customer.Mobile,
                     TotalAmount = abdefg.FinancialDocument.Amount,
                     CreateDateTime = abdefg.FinancialDocument.CreatedDateTime,
                     WalletId = abdefg.wallet.Id,
                     WalletName = abdefg.wallet.Plan.Title,
                     OrganizationName = abdefg.wallet.WalletContract.Organization.Title,
                     CashAmount = abdefg.FinancialDocument.FinancialDocumentPayments.Where(p => p.Type == FinancialDocumentPaymentType.Cash || p.Type == FinancialDocumentPaymentType.Prepayment).Sum(p => p.Amount),
                     CreditAmount = abdefg.FinancialDocument.FinancialDocumentPayments.Where(p => p.Type == FinancialDocumentPaymentType.Credit).Sum(p => p.Amount),
                     BranchName = merchantBranch.Title,
                     MerchantName = abdefg.merchant.Title,
                     State = abdefg.FinancialDocument.State,
                     PaymentGatewayType = abdefg.FinancialDocument.PaymentGatewayType,
                 }).FirstOrDefaultAsync(cancellationToken);
        return query;
    }

    public async Task<GetPurchasesListVm> GetPurchasesListAsync(
    GetPurchasesListQuery filter,
        CancellationToken cancellationToken)
    {
        var query = _context.FinancialDocuments.Where(f =>
        f.Type == FinancialDocumentType.Purchase
        && f.TenantId == filter.TenantId);


        if (filter.StartDate.HasValue && filter.StartDate != DateTime.MinValue)
        {
            query = query.Where(x => x.CreatedDateTime.Date >= filter.StartDate.Value.Date);
        }

        if (filter.EndDate.HasValue && filter.EndDate != DateTime.MinValue)
        {
            query = query.Where(x => x.CreatedDateTime.Date <= filter.EndDate.Value.Date);
        }

        if (!string.IsNullOrEmpty(filter.SearchValue))
        {
            query = query.Where(x => x.PaymentId.ToString().Contains(filter.SearchValue.Trim()));
        }

        if (filter.MerchantIds.Count > 0)
        {
            query = query.Where(f => filter.MerchantIds.Contains(f.ToBusinessIdentityId));
        }

        var queryCount = await query.CountAsync(cancellationToken);

        var purchases = await query
            .OrderByDescending(p => p.Id)
            .Skip((filter.PageIndex - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(x => new PurchaseVm()
            {
                Id = x.Id,
                CreateDateTime = x.CreatedDateTime,
                TotalAmount = x.Amount,
                MerchantName = x.MerchantBranch.Merchant.Title,
                OrganizationName = x.FinancialDocumentPayments.First().WalletContract.Organization.Title,
                ReferenceNumber = x.PaymentId.ToString(),
                State = x.State

            }).ToListAsync(cancellationToken);


        var summary = new GetPurchasesListVm
        {
            Items = purchases,
            PageIndex = filter.PageIndex,
            PageSize = filter.PageSize,
            TotalCount = queryCount
        };
        return summary;

    }
    public async Task<GetMerchantPurchasesQueryModel> GetPurchasesByMerchantIdAsync(GetMerchantPurchasesQuery filter)
    {
        var query = _context.FinancialDocuments
            .Include(x => x.MerchantBranch)
            .Join(_context.Customers,
            fm => fm.FromBusinessIdentityId,
            customer => customer.Id,
            (fm, customer) => new { FinancialDocument = fm, Customer = customer })
            .Where(f => f.FinancialDocument.Type == FinancialDocumentType.Purchase
            && (!filter.TenantId.HasValue || f.FinancialDocument.TenantId == filter.TenantId)
            && f.FinancialDocument.CreatedDateTime >= (filter.StartDate ?? DateTime.MinValue)
            && f.FinancialDocument.CreatedDateTime <= (filter.EndDate ?? DateTime.MaxValue));


        if (filter.MerchantBranchIds != null && filter.MerchantBranchIds.Any())
        {
            query = query.Where(f => filter.MerchantBranchIds.Contains(f.FinancialDocument.MerchantBranchId.Value));
        }
        if (!string.IsNullOrEmpty(filter.SearchValue))
        {
            query = query.Where(f => f.Customer.Mobile == filter.SearchValue ||
                        f.FinancialDocument.PaymentId.ToString() == filter.SearchValue.Trim());
        }
        if (filter.MerchantId.HasValue)
        {
            query = query.Where(f => f.FinancialDocument.ToBusinessIdentityId == filter.MerchantId.Value);
        }
        if (filter.Wallets != null && filter.Wallets.Any())
        {
            query = query.Where(f => f.FinancialDocument.FinancialDocumentPayments.Any(x => filter.Wallets.Contains(x.Wallet.Id)));
        }

        var queryCount = await query.CountAsync();

        var purchases = await query.Select(c => new GetMerchantPurchaseQueryModel
        {
            BranchName = c.FinancialDocument.MerchantBranch.Title,
            CreateDateTime = c.FinancialDocument.CreatedDateTime,
            Id = c.FinancialDocument.Id,
            Mobile = c.Customer.Mobile,
            PaymentId = c.FinancialDocument.PaymentId.Value,
            TotalAmount = c.FinancialDocument.Amount
        }).OrderByDescending(p => p.Id)
            .Skip((filter.PageIndex - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var result = new GetMerchantPurchasesQueryModel
        {
            Items = purchases,
            PageIndex = filter.PageIndex,
            PageSize = filter.PageSize,
            TotalCount = queryCount
        };
        return result;
    }

    public async Task<RefundFinancialDocumentQueryModel> GetFinanialDocumentRefundDetailByIdAsync(long id, int merchantId, int? merchantBranchId = null, int? tenantId = null)
    {
        RefundFinancialDocumentQueryModel refundFinancialDocumentQuery = new();

        var financialDocuments = await _context.FinancialDocuments
        .Where(x => x.ToBusinessIdentityId == merchantId && (!merchantBranchId.HasValue || x.MerchantBranchId == merchantBranchId) && (x.Id == id || x.ParentId == id) && (!tenantId.HasValue || x.TenantId == tenantId)).ToListAsync();

        var financialDocument = financialDocuments.FirstOrDefault(x => x.Id == id);
        var refundFinancialDocuments = financialDocuments.Where(x => x.ParentId == id && x.Type == FinancialDocumentType.Refund);

        refundFinancialDocumentQuery = new RefundFinancialDocumentQueryModel
        {
            FinancialDocumentId = financialDocument.Id,
            Amount = financialDocument.Amount,
            RemainAmount = financialDocument.Amount - refundFinancialDocuments.Sum(x => x.Amount)
        };

        if (refundFinancialDocuments.Any())
        {
            foreach (var refundFinancialDocument in refundFinancialDocuments)
            {
                var refundFinancialDocumentDetailQueryModel = new RefundFinancialDocumentDetailQueryModel
                {
                    RefundFinancialDocumentId = refundFinancialDocument.Id,
                    RefundAmount = refundFinancialDocument.Amount,
                    RemainAmount = financialDocument.Amount - refundFinancialDocuments.Where(x => x.Id <= refundFinancialDocument.Id).Sum(x => x.Amount),
                    RefundDescription = refundFinancialDocument.RefundDescription,
                    RefundReason = refundFinancialDocument.RefundReason.Value,
                    RefundDateTime = refundFinancialDocument.CreatedDateTime
                };

                refundFinancialDocumentQuery.RefundDetails.Add(refundFinancialDocumentDetailQueryModel);
            }
        }

        return refundFinancialDocumentQuery;
    }

    #region private_methods
    private async Task GetFinancialDocumentDetailAsync(FinancialDocument financialDocument, CustomerFinancialDocumentDetailQueryModel result, CancellationToken cancellationToken)
    {
        switch (financialDocument.Type)
        {
            case FinancialDocumentType.Purchase:
                result.PurchaseDetail = await GetPurchaseDetail(financialDocument.Id, cancellationToken);
                break;

            case FinancialDocumentType.WalletCharge:
                result.WalletChargeDetail = await GetWalletChargeDetail(financialDocument.Id, cancellationToken);
                break;

            case FinancialDocumentType.OperationalFee:
                result.OperationalFeeDetail = await GetOperationalFeeDetail(financialDocument.Id, cancellationToken);
                break;

            case FinancialDocumentType.Billing:
                result.BillingDetail = await GetBillingDetail(financialDocument.Id, cancellationToken);
                break;

            case FinancialDocumentType.VerificationFee:
                result.VerificationFeeDetail = await GetBaseFinancialDocumentDetail(financialDocument.Id, cancellationToken);
                break;

            case FinancialDocumentType.Refund:
                result.RefundDetail = await GetRefundDetail(financialDocument.Id, cancellationToken);
                break;

            case FinancialDocumentType.WithdrawCashOut:
                result.WithdrawCashOutDetail = await GetWithdrawCashOutDetail(financialDocument.Id, cancellationToken);
                break;
            default:
                break;
        }
    }

    private async Task<BillingDocumentDetail> GetBillingDetail(long financialDocumentId, CancellationToken cancellationToken)
    {
        var financialDocumentPayment = await _context.FinancialDocumentPayments
            .Include(x => x.Wallet).ThenInclude(x => x.Plan)
            .FirstOrDefaultAsync(p => p.FinancialDocumentId == financialDocumentId);

        var billingPayment = await _context
            .BillingPayments
            .Include(x => x.Billing).ThenInclude(x => x.FromAccount)
            .Where(c => c.TransactionId == financialDocumentPayment.TransactionId)
            .Join(_context.Wallets,
            bp => bp.Billing.FromAccountId, l => l.AccountId, (bp, l) => new BillingDocumentDetail
            {
                BillingDate = bp.Billing.EndDate,
                WalletName = l.Plan.Title,
                TrackingCode = string.Empty,
                SettlementType = bp.Billing.SettlementType
            }).FirstOrDefaultAsync(cancellationToken);

        return new BillingDocumentDetail
        {
            BillingDate = billingPayment.BillingDate,
            BillingWalletName = billingPayment.WalletName,
            WalletName = billingPayment.SettlementType == WalletSettlementType.Cheque ? "چک تسویه" : financialDocumentPayment.Wallet.Plan.Title,
            TrackingCode = string.Empty //TODO
        };
    }

    private async Task<OperationalFeeDocumentDetail> GetOperationalFeeDetail(long financialDocumentId, CancellationToken cancellationToken)
    {
        var result = await _context.FinancialDocumentPayments
            .Where(x => x.FinancialDocumentId == financialDocumentId)
            .Include(x => x.Wallet).ThenInclude(x => x.Plan)
            .Select(x => new OperationalFeeDocumentDetail
            {
                OperationalFeeType = OperationalFeeType.OnlinePayment,
                WalletName = x.Wallet != null ? x.Wallet.Plan.Title : string.Empty,
                TrackingCode = string.Empty
            }).FirstOrDefaultAsync(cancellationToken);

        return result;
    }

    private async Task<BaseFinancialDocumentDetail> GetBaseFinancialDocumentDetail(long financialDocumentId, CancellationToken cancellationToken)
    {
        var result = await _context.FinancialDocumentPayments
            .Where(x => x.FinancialDocumentId == financialDocumentId)
            .Include(x => x.Wallet).ThenInclude(x => x.Plan)
            .Select(x => new BaseFinancialDocumentDetail
            {
                WalletName = x.Wallet != null ? x.Wallet.Plan.Title : string.Empty,
            }).FirstOrDefaultAsync(cancellationToken);

        return result;
    }

    private async Task<WalletChargeDocumentDetail> GetWalletChargeDetail(long financialDocumentId, CancellationToken cancellationToken)
    {
        return await _context
            .FinancialDocumentPayments.Where(c => c.FinancialDocumentId == financialDocumentId)
            .Join(_context.CashWallets,
            fdp => fdp.WalletId, lw => lw.Id, (fdp, lw) => new WalletChargeDocumentDetail
            {
                WalletName = lw.Plan.Title,
                TrackingCode = string.Empty //TODO
            }).FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<PurchaseDocumentDetail> GetPurchaseDetail(long financialDocumentId, CancellationToken cancellationToken)
    {
        PurchaseDocumentDetail purchaseDocumentDetail = new();

        var financialDocumentPayments = await _context.FinancialDocumentPayments
            .Include(p => p.Wallet).ThenInclude(p => p.Plan)
            .Include(x => x.FinancialDocument).ThenInclude(x => x.MerchantBranch).ThenInclude(x => x.Merchant)
            .Where(c => c.FinancialDocumentId == financialDocumentId || (c.FinancialDocument.Type == FinancialDocumentType.Refund && c.FinancialDocument.ParentId == financialDocumentId))
            .ToListAsync(cancellationToken);

        if (financialDocumentPayments.Count > 0)
        {
            var cashAndPrepayment = financialDocumentPayments.Where(p => (p.Type == FinancialDocumentPaymentType.Cash || p.Type == FinancialDocumentPaymentType.Prepayment) && p.FinancialDocument.ParentId == null).ToList();
            var credit = financialDocumentPayments.Where(p => p.Type == FinancialDocumentPaymentType.Credit && p.FinancialDocument.ParentId == null).FirstOrDefault();

            if (cashAndPrepayment.Count != 0 || credit != null)
            {
                purchaseDocumentDetail = new PurchaseDocumentDetail
                {
                    MerchantName = cashAndPrepayment.Count != 0 ? cashAndPrepayment.First().FinancialDocument.MerchantBranch?.Merchant.Title : credit.FinancialDocument.MerchantBranch?.Merchant.Title,
                    BranchName = cashAndPrepayment.Count != 0 ? cashAndPrepayment.First().FinancialDocument.MerchantBranch?.Title : credit.FinancialDocument.MerchantBranch?.Title,
                    BranchCode = cashAndPrepayment.Count != 0 ? cashAndPrepayment.First().FinancialDocument.MerchantBranch?.TerminalId.ToString() : credit.FinancialDocument.MerchantBranch?.TerminalId.ToString(),
                    CashDeposit = cashAndPrepayment.Count != 0 ? new PaymentDetail
                    {
                        Amount = cashAndPrepayment.Sum(x => x.Amount),
                        WalletName = cashAndPrepayment.First().Wallet?.Plan?.Title
                    } : null,

                    CreditDeposit = credit != null ? new PaymentDetail
                    {
                        Amount = credit.Amount,
                        WalletName = credit.Wallet?.Plan?.Title
                    } : null
                };
            }

            var refunds = financialDocumentPayments.Where(p => p.FinancialDocument.ParentId == financialDocumentId).ToList();
            var refundDetails = new List<RefundDocumentDetail>();

            if (refunds.Count > 0)
            {
                purchaseDocumentDetail.RefundDetails = [];
                foreach (var item in refunds.GroupBy(x => x.FinancialDocumentId))
                {
                    var refundCashAndPrepayment = refunds.Where(p => (p.Type == FinancialDocumentPaymentType.Cash || p.Type == FinancialDocumentPaymentType.Prepayment) && p.FinancialDocumentId == item.Key).ToList();
                    var refundCredit = refunds.Where(p => p.Type == FinancialDocumentPaymentType.Credit && p.FinancialDocumentId == item.Key).FirstOrDefault();

                    if (refundCashAndPrepayment.Count != 0 || refundCredit != null)
                    {
                        refundDetails.Add(new RefundDocumentDetail
                        {
                            ParentId = refundCashAndPrepayment.Count != 0 ? refundCashAndPrepayment.First().FinancialDocument.ParentId : refundCredit.FinancialDocument.ParentId,
                            MerchantName = refundCashAndPrepayment.Count != 0 ? refundCashAndPrepayment.First().FinancialDocument.MerchantBranch?.Merchant.Title : refundCredit.FinancialDocument.MerchantBranch?.Merchant.Title,
                            BranchName = refundCashAndPrepayment.Count != 0 ? refundCashAndPrepayment.First().FinancialDocument.MerchantBranch?.Title : refundCredit.FinancialDocument.MerchantBranch?.Title,
                            BranchCode = refundCashAndPrepayment.Count != 0 ? refundCashAndPrepayment.First().FinancialDocument.MerchantBranch?.TerminalId.ToString() : refundCredit.FinancialDocument.MerchantBranch?.TerminalId.ToString(),
                            CashDeposit = refundCashAndPrepayment.Count != 0 ? new PaymentDetail
                            {
                                Amount = refundCashAndPrepayment.Sum(x => x.Amount),
                                WalletName = refundCashAndPrepayment.First().Wallet?.Plan?.Title
                            } : null,

                            CreditDeposit = refundCredit != null ? new PaymentDetail
                            {
                                Amount = refundCredit.Amount,
                                WalletName = refundCredit.Wallet?.Plan?.Title
                            } : null
                        });
                    }
                }
                purchaseDocumentDetail.RefundDetails = refundDetails;
            }

            return purchaseDocumentDetail;
        }
        return null;
    }

    private async Task<RefundDocumentDetail> GetRefundDetail(long financialDocumentId, CancellationToken cancellationToken)
    {
        var financialDocumentPayments = await _context.FinancialDocumentPayments
            .Include(p => p.Wallet).ThenInclude(p => p.Plan)
            .Include(x => x.FinancialDocument).ThenInclude(x => x.MerchantBranch).ThenInclude(x => x.Merchant)
            .Where(c => c.FinancialDocumentId == financialDocumentId)
            .ToListAsync(cancellationToken);

        if (financialDocumentPayments.Count > 0)
        {
            var cashAndPrepayment = financialDocumentPayments.Where(p => (p.Type == FinancialDocumentPaymentType.Cash || p.Type == FinancialDocumentPaymentType.Prepayment)).ToList();
            var credit = financialDocumentPayments.Where(p => p.Type == FinancialDocumentPaymentType.Credit).FirstOrDefault();

            if (cashAndPrepayment.Count != 0 || credit != null)
            {
                return new RefundDocumentDetail
                {
                    ParentId = cashAndPrepayment.Count != 0 ? cashAndPrepayment.First().FinancialDocument.ParentId : credit.FinancialDocument.ParentId,
                    MerchantName = cashAndPrepayment.Count != 0 ? cashAndPrepayment.First().FinancialDocument.MerchantBranch?.Merchant.Title : credit.FinancialDocument.MerchantBranch?.Merchant.Title,
                    BranchName = cashAndPrepayment.Count != 0 ? cashAndPrepayment.First().FinancialDocument.MerchantBranch?.Title : credit.FinancialDocument.MerchantBranch?.Title,
                    BranchCode = cashAndPrepayment.Count != 0 ? cashAndPrepayment.First().FinancialDocument.MerchantBranch?.TerminalId.ToString() : credit.FinancialDocument.MerchantBranch?.TerminalId.ToString(),
                    CashDeposit = cashAndPrepayment.Count != 0 ? new PaymentDetail
                    {
                        Amount = cashAndPrepayment.Sum(x => x.Amount),
                        WalletName = cashAndPrepayment.First().Wallet?.Plan?.Title
                    } : null,
                    CreditDeposit = credit != null ? new PaymentDetail
                    {
                        Amount = credit.Amount,
                        WalletName = credit.Wallet?.Plan?.Title
                    } : null
                };
            }
        }
        return null;
    }

    private async Task<WithdrawCashOutDocumentDetail> GetWithdrawCashOutDetail(long financialDocumentId, CancellationToken cancellationToken)
    {
        var financialDocumentPayments = await _context.FinancialDocumentPayments
            .Include(p => p.Wallet).ThenInclude(p => p.Plan)
            .Where(c => c.FinancialDocumentId == financialDocumentId)
            .ToListAsync(cancellationToken);

        if (financialDocumentPayments.Count > 0)
        {
            var cash = financialDocumentPayments.Where(p => p.Type == FinancialDocumentPaymentType.Cash).FirstOrDefault();

            if (cash != null)
            {
                return new WithdrawCashOutDocumentDetail
                {
                    CashDeposit = cash != null ? new PaymentDetail
                    {
                        Amount = cash.Amount,
                        WalletName = cash.Wallet?.Plan?.Title
                    } : null
                };
            }
        }
        return null;
    }

    #endregion
}