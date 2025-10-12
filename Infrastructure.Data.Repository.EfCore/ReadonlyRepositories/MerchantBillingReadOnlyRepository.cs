using Application.Query.Queries.MerchantBilling;
using Application.Query.QueryModels.MerchantBillings;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Billings;
using Application.Query.ViewModels.MerchantBillings;
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;
using Microsoft.EntityFrameworkCore;
using Shared.Utilities.Extensions;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repository.EfCore.ReadonlyRepositories;

public sealed class MerchantBillingReadOnlyRepository(ReadonlyApplicationDbContext dbContext) : IMerchantBillingReadOnlyRepository
{
    public async Task<GetBillingsViewModel> GetBillingsAsync(GetMerchantBillingsQuery query)
    {
        var billingQuery = dbContext.MerchantBillings.Where(p =>
            p.FromBusinessIdentityId == query.TenantId && p.ToBusinessIdentityId == query.MerchantId);

        if (query.Status.HasValue)
        {
            billingQuery = billingQuery.Where(p => p.Status == query.Status);
        }

        if (!string.IsNullOrEmpty(query.Code))
        {
            billingQuery = billingQuery.Where(p => p.Code.Contains(query.Code));
        }

        var totalCount = await billingQuery.CountAsync();

        var billings = await billingQuery.Select(p => new GetBillingsItemViewModel
        {
            Id = p.Id,
            Code = p.Code,
            Type = p.Type,
            Status = p.Status,
            DueDate = p.DueDate,
            EndDate = p.EndDate,
            TypeTitle = p.Type.GetEnumDescription(),
            StatusTitle = p.Status.GetEnumDescription(),
            PayableAmount = p.Amount - p.Payments.Sum(q => q.Amount)
        })
        .Skip((query.PageIndex - 1) * query.PageSize)
        .Take(query.PageSize)
        .ToListAsync();

        return new GetBillingsViewModel
        {
            Items = billings,
            TotalCount = totalCount,
            PageSize = query.PageSize,
            PageIndex = query.PageIndex
        };
    }

    public async Task<GetMerchantBillingViewModel> GetBillingAsync(GetMerchantBillingQuery query)
    {
        return await dbContext.MerchantBillings
            .Where(p => p.Id == query.Id && p.FromBusinessIdentityId == query.TenantId)
            .Select(p => new GetMerchantBillingViewModel
            {
                Id = p.Id,
                Code = p.Code,
                Status = p.Status,
                EndDate = p.EndDate,
                DueDate = p.DueDate,
                StartDate = p.StartDate,
                IsPayable = p.Amount > 0,
                PeriodType = p.PeriodType,
                Additions = p.AdditionsAmount,
                Deductions = p.DeductionsAmount,
                MerchantId = p.ToBusinessIdentityId,
                PaidAmount = p.Payments.Sum(q => q.Amount),
                StatusTitle = p.Status.GetEnumDescription(),
                PreviousDebitAmount = p.PreviousDebitAmount,
                PreviousCreditAmount = p.PreviousCreditAmount,
                PreviousPenaltyAmount = p.PreviousPenaltyAmount,
                PeriodTypeTitle = p.PeriodType.GetEnumDescription(),
                PayableAmount = p.Amount - p.Payments.Sum(q => q.Amount),
                PurchaseTransactionsAmount = p.PurchaseTransactionsAmount,
                RefundedTransactionsAmount = p.RefundedTransactionsAmount,
                PurchaseTransactionsCommission = p.PurchaseTransactionsCommission,
                RefundedTransactionsCommission = p.RefundedTransactionsCommission,
                TotalDebitAmount = p.PreviousDebitAmount + p.PurchaseTransactionsAmount + p.RefundedTransactionsCommission + p.AdditionsAmount,
                TotalCreditAmount = p.PreviousCreditAmount + p.PurchaseTransactionsCommission + p.RefundedTransactionsAmount + p.DeductionsAmount,
                Title = $"صورتحساب دوره ای {dbContext.Merchants.FirstOrDefault(q => q.Id == p.ToBusinessIdentityId).Title}",
                Payments = p.Payments.Select(q => new GetBillingPaymentViewModel()
                {

                })
            }).FirstOrDefaultAsync();
    }

    public async Task<GetPreviousDebitViewModel> GetPreviousDebitAsync(GetPreviousDebitQuery query)
    {
        return await dbContext.MerchantBillings
            .Where(p => p.Id == query.Id && p.FromBusinessIdentityId == query.TenantId)
            .Select(p => new GetPreviousDebitViewModel
            {
                Id = p.Debtor.Id,
                EndDate = p.Debtor.EndDate,
                StartDate = p.Debtor.StartDate,
                Amount = p.Debtor.Amount - p.Debtor.Payments.Sum(q => q.Amount)
            }).FirstOrDefaultAsync();
    }

    public async Task<GetPreviousCreditViewModel> GetPreviousCreditAsync(GetPreviousCreditQuery query)
    {
        return await dbContext.MerchantBillings
            .Where(p => p.Id == query.Id && p.FromBusinessIdentityId == query.TenantId)
            .Select(p => new GetPreviousCreditViewModel
            {
                Id = p.Creditor.Id,
                EndDate = p.Creditor.EndDate,
                StartDate = p.Creditor.StartDate,
                Amount = p.Creditor.Amount - p.Creditor.Payments.Sum(q => q.Amount)
            }).FirstOrDefaultAsync();
    }

    public async Task<GetAdditionsViewModel> GetAdditionsAsync(GetAdditionsQuery query)
    {
        return await dbContext.MerchantBillings
            .Where(p => p.Id == query.Id && p.FromBusinessIdentityId == query.TenantId)
            .Select(p => new GetAdditionsViewModel
            {
                Id = p.Id,
                Amount = p.Amount,
            }).FirstOrDefaultAsync();
    }

    public async Task<GetDeductionsViewModel> GetDeductionsAsync(GetDeductionsQuery query)
    {
        return await dbContext.MerchantBillings
            .Where(p => p.Id == query.Id && p.FromBusinessIdentityId == query.TenantId)
            .Select(p => new GetDeductionsViewModel
            {
                Id = p.Id,
                Amount = p.Amount,
            }).FirstOrDefaultAsync();
    }

    public async Task<GetPurchaseTransactionsViewModel> GetPurchaseTransactionsAsync(GetPurchaseTransactionsQuery query)
    {
        return await dbContext.MerchantBillings
            .Where(p => p.Id == query.Id && p.FromBusinessIdentityId == query.TenantId)
            .Select(p => new GetPurchaseTransactionsViewModel
            {
                Id = p.Id,
                Amount = p.PurchaseTransactionsAmount
            }).FirstOrDefaultAsync();
    }

    public async Task<GetRefundedTransactionsViewModel> GetRefundedTransactionsAsync(GetRefundedTransactionsQuery query)
    {
        return await dbContext.MerchantBillings
            .Where(p => p.Id == query.Id && p.FromBusinessIdentityId == query.TenantId)
            .Select(p => new GetRefundedTransactionsViewModel
            {
                Id = p.Id,
                Amount = p.RefundedTransactionsAmount
            }).FirstOrDefaultAsync();
    }

    public async Task<GetPurchaseTransactionsCommissionViewModel> GetPurchaseTransactionsCommissionAsync(GetPurchaseTransactionsCommissionQuery query)
    {
        return await dbContext.MerchantBillings
            .Where(p => p.Id == query.Id && p.FromBusinessIdentityId == query.TenantId)
            .Select(p => new GetPurchaseTransactionsCommissionViewModel
            {
                Id = p.Id,
                Amount = p.Amount,
                TransactionCount = 8058,
                PurchaseTransactionsCommission = p.PurchaseTransactionsAmount,
                CalculatedCommission = p.PurchaseTransactionsCalculatedCommission,
                Contracts = dbContext.TenantMerchantContracts.OrderByDescending(q => q.CreatedDateTime)
                    .Where(q => p.ContractIds.Contains(q.Id)).Select(q => new GetMerchantBillingContractViewModel()
                    {
                        Id = q.Id,
                        Status = q.Status,
                        EndDate = q.EndDate,
                        StartDate = q.StartDate,
                        FixedAmountCommission = q.FixedAmountCommission,
                        FixedPercentageCommission = q.FixedPercentageCommission,
                        PeriodMaxCommissionAmount = q.PeriodMaxCommissionAmount,
                        PeriodMinCommissionAmount = q.PeriodMinCommissionAmount,
                        TransactionMaxCommissionAmount = q.TransactionMaxCommissionAmount,
                        TransactionMinCommissionAmount = q.TransactionMinCommissionAmount,
                        CommissionCalculationType = q.CommissionCalculationType,
                        CommissionCalculationTypeTitle = q.CommissionCalculationType.GetEnumDescription(),
                        TieredCommissions = q.TieredCommissions.Select(r => new TieredCommissionViewModel()
                        {
                            FromAmount = r.FromAmount,
                            ToAmount = r.ToAmount,
                            MaxAmount = r.MaxAmount,
                            MinAmount = r.MinAmount,
                            Percentage = r.Percentage
                        })
                    })
            }).FirstOrDefaultAsync();
    }

    public async Task<GetRefundedTransactionsCommissionViewModel> GetRefundedTransactionsCommissionAsync(GetRefundedTransactionsCommissionQuery query)
    {
        return await dbContext.MerchantBillings
            .Where(p => p.Id == query.Id && p.FromBusinessIdentityId == query.TenantId)
            .Select(p => new GetRefundedTransactionsCommissionViewModel
            {
                Id = p.Id,
                Amount = p.RefundedTransactionsCommission,
                Contracts = dbContext.TenantMerchantContracts.OrderByDescending(q => q.CreatedDateTime)
                    .Where(q => p.ContractIds.Contains(q.Id)).Select(q => new GetMerchantBillingContractViewModel()
                    {
                        Id = q.Id,
                        Status = q.Status,
                        EndDate = q.EndDate,
                        StartDate = q.StartDate,
                        FixedAmountCommission = q.FixedAmountCommission,
                        FixedPercentageCommission = q.FixedPercentageCommission,
                        PeriodMaxCommissionAmount = q.PeriodMaxCommissionAmount,
                        PeriodMinCommissionAmount = q.PeriodMinCommissionAmount,
                        TransactionMaxCommissionAmount = q.TransactionMaxCommissionAmount,
                        TransactionMinCommissionAmount = q.TransactionMinCommissionAmount,
                        CommissionCalculationType = q.CommissionCalculationType,
                        CommissionCalculationTypeTitle = q.CommissionCalculationType.GetEnumDescription(),
                        TieredCommissions = q.TieredCommissions.Select(r => new TieredCommissionViewModel()
                        {
                            FromAmount = r.FromAmount,
                            ToAmount = r.ToAmount,
                            MaxAmount = r.MaxAmount,
                            MinAmount = r.MinAmount,
                            Percentage = r.Percentage
                        })
                    })
            }).FirstOrDefaultAsync();
    }

    public async Task<GetMerchantBillingQueryModel> GetBillingByIdAsync(long id, int tenantId)
    {
        var billing = await dbContext.MerchantBillings.FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);

        return billing == null ? null : new GetMerchantBillingQueryModel
        {
            Id = billing.Id,
            DueDate = billing.DueDate,
            EndDate = billing.EndDate,
            GracePeriod = billing.GracePeriod,
            Status = billing.Status,
            PayableAmount = billing.GetPayableAmount()
        };
    }
}