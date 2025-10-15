using System.Linq;
using Domain.Core.Enums;
using Domain.Core.Constants;
using System.Threading.Tasks;
using Application.Service.Helper;
using System.Collections.Generic;
using Domain.Core.Entities.Shared;
using Shared.Utilities.Extensions;
using Microsoft.EntityFrameworkCore;
using Application.Query.ViewModels.Billings;
using Application.Query.Queries.MerchantBilling;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.MerchantBillings;
using Application.Query.QueryModels.MerchantBillings;
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;

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
            PaymentDeadlineDate = p.PaymentDeadlineDate,
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

    public async Task<GetMerchantBillingVm> GetBillingAsync(GetMerchantBillingQuery query)
    {
        return await dbContext.MerchantBillings
            .Where(p => p.Id == query.Id && p.FromBusinessIdentityId == query.TenantId)
            .Select(p => new GetMerchantBillingVm
            {
                Id = p.Id,
                Code = p.Code,
                Status = p.Status,
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

    public async Task<GetPreviousDebitVm> GetPreviousDebitAsync(GetPreviousDebitQuery query)
    {
        return await dbContext.MerchantBillings
            .Where(p => p.Id == query.Id && p.FromBusinessIdentityId == query.TenantId)
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
            .Where(p => p.Id == query.Id && p.FromBusinessIdentityId == query.TenantId)
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
            .Where(p => p.Id == query.Id && p.FromBusinessIdentityId == query.TenantId)
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
            .Where(p => p.Id == query.Id && p.FromBusinessIdentityId == query.TenantId)
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
            .Where(p => p.Id == query.Id && p.FromBusinessIdentityId == query.TenantId)
            .Select(p => new GetPurchaseTransactionsVm
            {
                Id = p.Id,
                ContractId = p.MainContractId,
                Amount = p.PurchaseTransactionsAmount
            }).FirstOrDefaultAsync();
    }

    public async Task<GetRefundedTransactionsVm> GetRefundedTransactionsAsync(GetRefundedTransactionsQuery query)
    {
        return await dbContext.MerchantBillings
            .Where(p => p.Id == query.Id && p.FromBusinessIdentityId == query.TenantId)
            .Select(p => new GetRefundedTransactionsVm
            {
                Id = p.Id,
                ContractId = p.MainContractId,
                Amount = p.RefundedTransactionsAmount
            }).FirstOrDefaultAsync();
    }

    public async Task<GetPurchaseTransactionsCommissionVm> GetPurchaseTransactionsCommissionAsync(GetPurchaseTransactionsCommissionQuery query)
    {
        var billing = await dbContext.MerchantBillings
            .Where(p => p.Id == query.Id && p.FromBusinessIdentityId == query.TenantId)
            .Select(p => new GetPurchaseTransactionsCommissionVm
            {
                Id = p.Id,
                MainContractId = p.MainContractId,
                FinalAmount = p.PurchaseTransactionsCommission,
                TransactionsAmount = p.PurchaseTransactionsAmount,
                CalculatedAmount = p.PurchaseTransactionsCalculatedCommission,

                Contracts = dbContext.TenantMerchantContracts.OrderByDescending(q => q.EndDate)
                    .Where(q => p.ContractIds.Contains(q.Id)).Select(q => new GetMerchantBillingContractVm()
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
                            q.CommissionCalculationType == CommissionCalculationType.FixedAmount ? TenantMerchantConstants.FixedAmountCommissionDescription : string.Empty,

                    }).ToList(),

                TransactionsCount = dbContext.MerchantInstallments
                    .Count(q => q.Type == InstallmentType.Purchase &&
                                p.StartDate <= q.DueDate && q.DueDate < p.DueDate &&
                                p.ContractIds.Contains(q.TenantMerchantContractId)),

                Message = p.PurchaseTransactionsCommission > p.PurchaseTransactionsCalculatedCommission ?
                    $".مجموع کارمزد شما {p.PurchaseTransactionsCalculatedCommission} ریال است که از حداقل مبلغ کارمزد دوره کمتر است، در نتیجه حداقل مبلغ کارمزد یعنی {p.PurchaseTransactionsCommission} درنظر گرفته می شود" :
                    string.Empty

            }).FirstOrDefaultAsync();

        var mainContract = billing.Contracts.First(p => p.Id == billing.MainContractId);

        var tieredCommissionLevels = GetCumulativeTieredCommissionLevels(mainContract.TieredCommissions, billing.TransactionsAmount);

        billing.TieredCommissionLevels = tieredCommissionLevels;

        return billing;
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
            .Where(p => p.Id == query.Id && p.FromBusinessIdentityId == query.TenantId)
            .Select(p => new GetRefundedTransactionsCommissionVm
            {
                Id = p.Id,
                Amount = p.RefundedTransactionsCommission,
                Contracts = dbContext.TenantMerchantContracts.OrderByDescending(q => q.EndDate)
                    .Where(q => contractIds.Contains(q.Id)).Select(q => new GetMerchantBillingContractVm()
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
        return await dbContext.MerchantBillings
            .Where(p => p.Id == id && p.FromBusinessIdentityId == tenantId)
            .Select(p => new GetBillingPeriodQueryModel()
            {
                DueDate = p.DueDate,
                StartDate = p.StartDate,
                ContractIds = p.ContractIds
            }).FirstOrDefaultAsync();
    }

    private static List<TieredCommissionLevel> GetCumulativeTieredCommissionLevels(List<TieredCommission> tieredCommissions, decimal totalTransactionsAmount)
    {
        if (tieredCommissions == null) return [];

        var number = 1;

        List<TieredCommissionLevel> tieredCommissionLevels = [];

        var remainingAmount = totalTransactionsAmount;

        foreach (var tieredCommission in tieredCommissions.OrderBy(p => p.FromAmount))
        {
            decimal transactionsAmount;

            decimal calculatedCommission;

            if (tieredCommission.ToAmount == null)
            {
                transactionsAmount = remainingAmount;

                calculatedCommission = transactionsAmount * (tieredCommission.Percentage / 100);

                calculatedCommission = RoundHelper.RoundAmount(calculatedCommission);

                if (calculatedCommission > tieredCommission.MaxAmount)
                {
                    calculatedCommission = tieredCommission.MaxAmount.Value;
                }

                if (calculatedCommission < tieredCommission.MinAmount)
                {
                    calculatedCommission = tieredCommission.MinAmount.Value;
                }

                tieredCommissionLevels.Add(new TieredCommissionLevel(number, calculatedCommission, transactionsAmount));

                break;
            }

            var tieredTotalAmount = tieredCommission.ToAmount.Value - tieredCommission.FromAmount;

            if (remainingAmount >= tieredTotalAmount)
            {
                transactionsAmount = tieredTotalAmount;
            }
            else
            {
                transactionsAmount = remainingAmount;
            }

            calculatedCommission = transactionsAmount * (tieredCommission.Percentage / 100);

            calculatedCommission = RoundHelper.RoundAmount(calculatedCommission);

            if (calculatedCommission > tieredCommission.MaxAmount)
            {
                calculatedCommission = tieredCommission.MaxAmount.Value;
            }

            if (calculatedCommission < tieredCommission.MinAmount)
            {
                calculatedCommission = tieredCommission.MinAmount.Value;
            }

            tieredCommissionLevels.Add(new TieredCommissionLevel(number, calculatedCommission, transactionsAmount));

            remainingAmount -= tieredTotalAmount;

            if (remainingAmount <= 0) break;

            number++;
        }

        return tieredCommissionLevels;
    }
}