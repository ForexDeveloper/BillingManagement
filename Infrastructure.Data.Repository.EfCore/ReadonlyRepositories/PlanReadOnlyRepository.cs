using Application.Query.Queries;
using Application.Query.QueryModels;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repository.EfCore.ReadonlyRepositories
{
    public class PlanReadOnlyRepository(ReadonlyApplicationDbContext readonlyApplicationDbContext) : IPlanReadOnlyRepository
    {
        public async Task<List<PlanSimpleListQueryModel>> GetPlansSimpleList(int tenantId, CancellationToken cancellationToken)
        {
            return await readonlyApplicationDbContext.Plans.Where(p => p.WalletConfiguration.TenantId == tenantId)
                .Select(p => new PlanSimpleListQueryModel
                {
                    Id = p.Id,
                    Title = p.Title,
                }).ToListAsync(cancellationToken);
        }

        public async Task<PlanQueryModel> GetByIdAsync(int id, int? tenantId = null)
        {
            var result = await readonlyApplicationDbContext.Plans
                 .Where(x => x.Id == id && (!tenantId.HasValue || x.WalletConfiguration.TenantId == tenantId))
                 .Select(x => new PlanQueryModel
                 {
                     Id = x.Id,
                     MaxDailyDeposit = x.MaxDailyDeposit,
                     MaxDailyTransactionCount = x.MaxDailyTransactionCount,
                     MaxDailyWithdrawal = x.MaxDailyWithdrawal,
                     MaxTotalCredit = x.MaxTotalCredit,
                     MaxWallet = x.MaxWallet,
                     TenantId = x.WalletConfiguration.TenantId,
                     TenantTitle = x.WalletConfiguration.Tenant.Title,
                     Title = x.Title,
                     WalletConfigurationId = x.WalletConfigurationId,
                     WalletConfigurationTitle = x.WalletConfiguration.Title,
                     GracePeriod = x.GracePeriod,
                     InstallmentBreakType = x.InstallmentBreakType,
                     BillingPeriodType = x.BillingPeriodType,
                     BackgroundColor1 = x.BackgroundColor1,
                     BackgroundColor2 = x.BackgroundColor2,
                     BillingPeriodStartDate = x.BillingPeriodStartDate,
                     Description = x.Description,
                     InstallmentPaymentMethod = x.InstallmentPaymentMethod,
                     Link = x.Link,
                     PaymentType = x.PaymentType,
                     TextColor = x.TextColor,
                     BillingPeriod = x.BillingPeriod,
                     InstallmentBreak = x.InstallmentBreak,
                     PlanClosedloops = x.PlanClosedloops.Where(c => !c.IsDeleted).Select(c => new PlanClosedloopQueryModel
                     {
                         Id = c.Id,
                         ClosedloopId = c.ClosedLoopId,
                         Title = c.ClosedLoop.Title,
                     }),
                     TermsAndConditions = x.TermsAndConditions,
                     PlanDetails = x.PlanDetails.Where(c => !c.IsDeleted).Select(m => new PlanDetailQueryModel
                     {
                         Id = m.Id,
                         PlanId = m.PlanId,
                         NumberOfInstallments = m.PlanDetailInstallments.Where(c => !c.IsDeleted).Select(c => c.NumberOfInstallment),
                         OperationFee = m.OperationFee,
                         OperationalFeeType = m.OperationalFeeType,
                         PenaltyPercent = m.PenaltyPercent,
                         PenaltyMaxAmount = m.PenaltyMaxAmount,
                         PenaltyMinAmount = m.PenaltyMinAmount,
                         InterestPercent = m.InterestPercent,
                         InterestMaxAmount = m.InterestMaxAmount,
                         InterestMinAmount = m.InterestMinAmount,
                         WaiverPercent = m.WaiverPercent,
                         WaiverMaxAmount = m.WaiverMaxAmount,
                         WaiverMinAmount = m.WaiverMinAmount,
                         PrepaymentPercent = m.PrepaymentPercent,
                         PrepaymentMaxAmount = m.PrepaymentMaxAmount,
                         PrepaymentMinAmount = m.PrepaymentMinAmount
                     }),
                     ReservedCredit = x.ReservedCredit,
                     AssignedCredit = x.AssignedCredit

                 }).FirstOrDefaultAsync();

            return result;
        }

        public async Task<bool> HasWalletContractPlanByPlanId(int id)
        => await readonlyApplicationDbContext.Plans
                .AnyAsync(c => c.Id == id && c.WalletContractPlans.Any(c => c.PlanId == id));

        public async Task<GetPlanForGridQueryModel> GetListAsync(GetAllPlanQuery query)
        {
            var closedloopQuery = readonlyApplicationDbContext.Plans.AsQueryable();

            if (query.TenantId > 0)
            {
                closedloopQuery = closedloopQuery.Where(c => c.WalletConfiguration.TenantId == query.TenantId);
            }
            if (query.WalletConfigurationId > 0)
            {
                closedloopQuery = closedloopQuery.Where(c => c.WalletConfigurationId == query.WalletConfigurationId);
            }
            if (!string.IsNullOrWhiteSpace(query.SearchValue))
            {
                query.SearchValue = query.SearchValue.Trim();
                closedloopQuery = closedloopQuery
                    .Where(x => x.Title.Contains(query.SearchValue));
            }
            var totalCounts = await closedloopQuery.CountAsync();

            closedloopQuery = query.SortColumn?.ToLower() switch
            {
                "maxtotalcredit" => query.SortDirection == Application.Query.Base.SortDirection.Ascending ?
                    closedloopQuery.OrderBy(x => x.MaxTotalCredit) : closedloopQuery.OrderByDescending(x => x.MaxTotalCredit),
                "maxwallet" => query.SortDirection == Application.Query.Base.SortDirection.Ascending ?
                    closedloopQuery.OrderBy(x => x.MaxWallet) : closedloopQuery.OrderByDescending(x => x.MaxWallet),
                _ => closedloopQuery.OrderByDescending(x => x.Id)
            };


            var result = await closedloopQuery
                .Skip((query.PageIndex - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(x => new GetAllPlanQueryModel()
                {
                    Id = x.Id,
                    Title = x.Title,
                    MaxTotalCredit = x.MaxTotalCredit,
                    MaxWallet = x.MaxWallet,
                    TenantId = x.WalletConfiguration.TenantId,
                    TenantTitle = x.WalletConfiguration.Tenant.Title,
                    WalletConfigurationTitle = x.WalletConfiguration.Title,
                    WalletTypeId = x.WalletConfiguration.WalletTypeId,
                    MaxDailyDeposit = x.MaxDailyDeposit,
                    MaxDailyTransactionCount = x.MaxDailyTransactionCount,
                    MaxDailyWithdrawal = x.MaxDailyWithdrawal,
                    TermsAndConditions = x.TermsAndConditions
                }).ToListAsync();


            return new GetPlanForGridQueryModel()
            {
                PageIndex = query.PageIndex,
                PageSize = query.PageSize,
                Items = result,
                TotalCount = totalCounts
            };
        }

        public async Task<bool> HasPlanByWalletConfigurationIdAsync(int walletConfigurationId)
       => await readonlyApplicationDbContext.Plans
          .AnyAsync(c => c.WalletConfigurationId == walletConfigurationId);

        public async Task<GetPlanInstallmentsQueryModel> GetAllInstallmentsAsync(GetPlanInstallmentsQuery query, CancellationToken cancellationToken)
        {
            return await readonlyApplicationDbContext.Plans.Where(p => p.Id == query.PlanId).Select(p => new GetPlanInstallmentsQueryModel
            {
                InstallmentBreak = p.InstallmentBreak,
                InstallmentBreakType = p.InstallmentBreakType,
                InstallmentInterestPercent = p.PlanDetails
                    .Where(q => q.PlanDetailInstallments.Any(r => r.NumberOfInstallment == query.NumberOfInstallments))
                    .Select(q => q.InterestPercent).FirstOrDefault(),
                OperationFee = p.PlanDetails
                    .Where(q => q.PlanDetailInstallments.Any(r => r.NumberOfInstallment == query.NumberOfInstallments))
                    .Select(q => q.OperationFee).FirstOrDefault(),
                BillingPeriod = p.BillingPeriod.GetValueOrDefault()
            }).FirstOrDefaultAsync(cancellationToken);
        }
    }
}