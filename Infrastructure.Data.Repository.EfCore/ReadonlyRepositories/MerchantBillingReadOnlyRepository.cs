using System.Linq;
using Domain.Core.Enums;
using Domain.Core.Helper;
using Domain.Core.Constants;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Application.Query.ViewModels.Billings;
using Application.Query.Queries.MerchantBilling;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.MerchantBillings;
using Application.Query.QueryModels.MerchantBillings;
using Domain.Core.Entities.BillingAggregate.Exceptions;
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;

namespace Infrastructure.Data.Repository.EfCore.ReadonlyRepositories;

public sealed class MerchantBillingReadOnlyRepository(ReadonlyApplicationDbContext dbContext) : IMerchantBillingReadOnlyRepository
{
    public async Task<GetBillingsVm> GetBillingsAsync(GetMerchantBillingsQuery query)
    {
        var billingQuery = dbContext.MerchantBillings.Where(p =>
            p.FromBusinessIdentityId == query.TenantId && p.ToBusinessIdentityId == query.MerchantId);

        if (query.Status.HasValue)
        {
            billingQuery = billingQuery.Where(p => p.Status == query.Status);
        }

        if (!string.IsNullOrEmpty(query.Code))
        {
            billingQuery = billingQuery.Where(p => p.Code.Contains(query.Code.Trim()));
        }

        var totalCount = await billingQuery.CountAsync();

        var billings = await billingQuery.Select(p => new GetBillingsItemVm
        {
            Id = p.Id,
            Code = p.Code,
            Type = p.Type,
            Status = p.Status,
            DueDate = p.DueDate,
            TypeTitle = p.Type.GetEnumDescription(),
            StatusTitle = p.Status.GetEnumDescription(),
            PaymentDeadlineDate = p.PaymentDeadlineDate,
            PayableAmount = p.Amount - p.Payments.Sum(q => q.Amount)
        })
        .Skip((query.PageIndex - 1) * query.PageSize)
        .Take(query.PageSize)
        .ToListAsync();

        return new GetBillingsVm
        {
            Items = billings,
            TotalCount = totalCount,
            PageSize = query.PageSize,
            PageIndex = query.PageIndex
        };
    }

    public async Task<GetMerchantBillingVm> GetBillingAsync(GetMerchantBillingQuery query)
    {
        return await dbContext.MerchantBillings
            .Where(p => p.Id == query.Id && p.TenantId == query.TenantId)
            .Select(p => new GetMerchantBillingVm
            {
                Id = p.Id,
                Code = p.Code,
                Type = p.Type,
                Amount = p.Amount,
                Status = p.Status,
                DueDate = p.DueDate,
                StartDate = p.StartDate,
                PeriodType = p.PeriodType,
                Additions = p.AdditionsAmount,
                Deductions = p.DeductionsAmount,
                TypeTitle = p.Type.GetEnumDescription(),
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
                IsPayable = p.Amount > 0 && (p.Status == BillingStatus.Issued || p.Status == BillingStatus.PartiallyPaid),
                IsCommissionExchanged = dbContext.TenantMerchantContracts.FirstOrDefault(q => q.Id == p.MainContractId).IsCommissionExchanged,
                TotalDebitAmount = p.PreviousDebitAmount + p.PurchaseTransactionsAmount + p.RefundedTransactionsCommission + p.AdditionsAmount,
                TotalCreditAmount = p.PreviousCreditAmount + p.PurchaseTransactionsCommission + p.RefundedTransactionsAmount + p.DeductionsAmount,

                MerchantId = p.Type == BillingType.TenantToMerchant ? p.ToBusinessIdentityId :
                             p.Type == BillingType.MerchantToTenant ? p.FromBusinessIdentityId : 0,

                Title = p.Type == BillingType.TenantToMerchant ? $"صورتحساب دوره ای {dbContext.Merchants.FirstOrDefault(q => q.Id == p.ToBusinessIdentityId).Title}" :
                        p.Type == BillingType.MerchantToTenant ? $"صورتحساب کارمزد {dbContext.Merchants.FirstOrDefault(q => q.Id == p.FromBusinessIdentityId).Title} به بهره بردار" : string.Empty

            }).FirstOrDefaultAsync();
    }

    public async Task<GetPreviousDebitVm> GetPreviousDebitAsync(GetPreviousDebitQuery query)
    {
        return await dbContext.MerchantBillings
            .Where(p => p.Id == query.Id && p.TenantId == query.TenantId)
            .Where(p => p.DebtorId.HasValue)
            .Select(p => new GetPreviousDebitVm
            {
                Id = p.Debtor.Id,
                EndDate = p.Debtor.DueDate,
                StartDate = p.Debtor.StartDate,
                Amount = p.PreviousDebitAmount
            }).FirstOrDefaultAsync();
    }

    public async Task<GetPreviousCreditVm> GetPreviousCreditAsync(GetPreviousCreditQuery query)
    {
        return await dbContext.MerchantBillings
            .Where(p => p.Id == query.Id && p.TenantId == query.TenantId)
            .Where(p => p.CreditorId.HasValue)
            .Select(p => new GetPreviousCreditVm
            {
                Id = p.Creditor.Id,
                EndDate = p.Creditor.DueDate,
                StartDate = p.Creditor.StartDate,
                Amount = p.PreviousCreditAmount
            }).FirstOrDefaultAsync();
    }

    public async Task<GetAdditionsVm> GetAdditionsAsync(GetAdditionsQuery query)
    {
        return await dbContext.MerchantBillings
            .Where(p => p.Id == query.Id && p.TenantId == query.TenantId)
            .Select(p => new GetAdditionsVm
            {
                Id = p.Id,
                Amount = p.AdditionsAmount,
                Description = p.AdditionsDescription
            }).FirstOrDefaultAsync();
    }

    public async Task<GetDeductionsVm> GetDeductionsAsync(GetDeductionsQuery query)
    {
        return await dbContext.MerchantBillings
            .Where(p => p.Id == query.Id && p.TenantId == query.TenantId)
            .Select(p => new GetDeductionsVm
            {
                Id = p.Id,
                Amount = p.DeductionsAmount,
                Description = p.DeductionsDescription
            }).FirstOrDefaultAsync();
    }

    public async Task<GetPurchaseTransactionsVm> GetPurchaseTransactionsAsync(GetPurchaseTransactionsQuery query)
    {
        return await dbContext.MerchantBillings
            .Where(p => p.Id == query.Id && p.TenantId == query.TenantId)
            .Select(p => new GetPurchaseTransactionsVm
            {
                Id = p.Id,
                ContractId = p.MainContractId,
                MerchantId = p.ToBusinessIdentityId,
                Amount = p.PurchaseTransactionsAmount
            }).FirstOrDefaultAsync();
    }

    public async Task<GetRefundedTransactionsVm> GetRefundedTransactionsAsync(GetRefundedTransactionsQuery query)
    {
        return await dbContext.MerchantBillings
            .Where(p => p.Id == query.Id && p.TenantId == query.TenantId)
            .Select(p => new GetRefundedTransactionsVm
            {
                Id = p.Id,
                ContractId = p.MainContractId,
                MerchantId = p.ToBusinessIdentityId,
                Amount = p.RefundedTransactionsAmount
            }).FirstOrDefaultAsync();
    }

    public async Task<GetPurchaseTransactionsCommissionQueryModel> GetPurchaseTransactionsCommissionAsync(GetPurchaseTransactionsCommissionQuery query)
    {
        return await dbContext.MerchantBillings
            .Where(p => p.Id == query.Id && p.TenantId == query.TenantId)
            .Select(p => new GetPurchaseTransactionsCommissionQueryModel
            {
                BillingId = p.Id,
                MainContractId = p.MainContractId,
                FinalAmount = p.PurchaseTransactionsCommission,
                TransactionsAmount = p.PurchaseTransactionsAmount,
                TieredCalculatedLevels = p.TieredCalculatedLevels,
                CalculatedAmount = p.PurchaseTransactionsCalculatedCommission,

                Contracts = dbContext.TenantMerchantContracts.OrderByDescending(q => q.Status).ThenByDescending(q => q.EndDate)
                    .Where(q => p.ContractIds.Contains(q.Id)).Select(q => new GetMerchantBillingContractQueryModel()
                    {
                        Id = q.Id,
                        Status = q.Status,
                        EndDate = q.EndDate,
                        StartDate = q.StartDate,
                        TieredCommissions = q.TieredCommissions,
                        FixedAmountCommission = q.FixedAmountCommission,
                        FixedPercentageCommission = q.FixedPercentageCommission,
                        PeriodMaxCommissionAmount = q.PeriodMaxCommissionAmount,
                        PeriodMinCommissionAmount = q.PeriodMinCommissionAmount,
                        TransactionMaxCommissionAmount = q.TransactionMaxCommissionAmount,
                        TransactionMinCommissionAmount = q.TransactionMinCommissionAmount,
                        CommissionCalculationType = q.CommissionCalculationType,
                        CommissionCalculationTypeTitle = q.CommissionCalculationType.GetEnumDescription(),
                        Description = q.CommissionCalculationType == CommissionCalculationType.UniformTiered ? TenantMerchantConstants.UniformedTieredCommissionDescription :
                            q.CommissionCalculationType == CommissionCalculationType.CumulativeTiered ? TenantMerchantConstants.CumulativeTieredCommissionDescription :
                            q.CommissionCalculationType == CommissionCalculationType.FixedPercentage ? TenantMerchantConstants.FixedPercentageCommissionDescription :
                            q.CommissionCalculationType == CommissionCalculationType.FixedAmount ? TenantMerchantConstants.FixedAmountCommissionDescription : string.Empty

                    }).ToList(),

                TransactionsCount = dbContext.MerchantInstallments
                    .Count(q => q.Type == InstallmentType.Purchase &&
                                p.StartDate <= q.DueDate && q.DueDate < p.DueDate &&
                                p.ContractIds.Contains(q.TenantMerchantContractId)),

                MerchantId = p.Type == BillingType.TenantToMerchant ? p.ToBusinessIdentityId : 
                             p.Type == BillingType.MerchantToTenant ? p.FromBusinessIdentityId : 0,

                Message = p.PurchaseTransactionsCommission > p.PurchaseTransactionsCalculatedCommission ? 
                    $".مجموع کارمزد شما {p.PurchaseTransactionsCalculatedCommission.Normalize()} ریال است که از حداقل مبلغ کارمزد دوره کمتر است، در نتیجه حداقل مبلغ کارمزد یعنی {p.PurchaseTransactionsCommission.Normalize()} درنظر گرفته می شود" :
                    string.Empty

            }).FirstOrDefaultAsync();
    }

    public async Task<GetRefundedTransactionsCommissionVm> GetRefundedTransactionsCommissionAsync(GetRefundedTransactionsCommissionQuery query)
    {
        var billing = await GetBillingPeriodAsync(query.Id, query.TenantId);

        var contractIds = dbContext.FinancialDocuments
            .Where(p => p.Type == FinancialDocumentType.Refund)
            .Where(p => billing.ContractIds.Contains(p.TenantMerchantContractId.Value))
            .Where(p => billing.StartDate <= p.CreatedDateTime && p.CreatedDateTime < billing.DueDate)
            .Where(p => p.Parent.TenantMerchantContract.CommissionCalculationType == CommissionCalculationType.FixedAmount ||
                        p.Parent.TenantMerchantContract.CommissionCalculationType == CommissionCalculationType.FixedPercentage)
            .Select(p => p.Parent.TenantMerchantContractId);

        return await dbContext.MerchantBillings
            .Where(p => p.Id == query.Id && p.TenantId == query.TenantId)
            .Select(p => new GetRefundedTransactionsCommissionVm
            {
                Id = p.Id,
                Amount = p.RefundedTransactionsCommission,
                Contracts = dbContext.TenantMerchantContracts.OrderByDescending(q => q.Status).ThenByDescending(q => q.EndDate)
                    .Where(q => contractIds.Contains(q.Id)).Select(q => new GetMerchantBillingContractVm()
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
                        Description = q.CommissionCalculationType == CommissionCalculationType.FixedAmount
                            ? TenantMerchantConstants.FixedAmountCommissionDescription
                            : TenantMerchantConstants.FixedPercentageCommissionDescription
                    }).ToList()
            }).FirstOrDefaultAsync();
    }

    public async Task<GetMerchantBillingQueryModel> GetBillingByIdAsync(long id, int tenantId)
    {
        var billing = await dbContext.MerchantBillings.FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);

        return billing == null ? null : new GetMerchantBillingQueryModel
        {
            Id = billing.Id,
            Status = billing.Status,
            DueDate = billing.DueDate,
            PaymentDeadlineDate = billing.PaymentDeadlineDate,
            GracePeriod = billing.GracePeriod,
            PayableAmount = billing.GetPayableAmount()
        };
    }

    private async Task<GetBillingPeriodQueryModel> GetBillingPeriodAsync(long id, int tenantId)
    {
        var billing = await dbContext.MerchantBillings
            .Where(p => p.Id == id && p.TenantId == tenantId)
            .Select(p => new GetBillingPeriodQueryModel()
            {
                DueDate = p.DueDate,
                StartDate = p.StartDate,
                ContractIds = p.ContractIds
            }).FirstOrDefaultAsync();

        if (billing == null)
        {
            throw new BillingNotFoundException(BillingConstants.NotFoundMessage);
        }

        return billing;
    }
}