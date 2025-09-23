using Domain.Core.Entities.TenantMerchantContractAggregate;
using Domain.Core.Entities.TenantMerchantContractAggregate.Dtos;
using Domain.Core.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;

namespace Infrastructure.Data.Repository.EfCore.Repositories;

public class TenantMerchantContractRepository(ApplicationDbContext applicationDbContext) : ITenantMerchantContractRepository
{
    private readonly ApplicationDbContext _applicationDbContext = applicationDbContext;

    public async Task AddAsync(TenantMerchantContract tenantMerchantContract)
    {
        await applicationDbContext.TenantMerchantContracts.AddAsync(tenantMerchantContract);
    }

    public void Update(TenantMerchantContract tenantMerchantContract)
    {
        applicationDbContext.TenantMerchantContracts.Update(tenantMerchantContract);
    }

    public async Task<TenantMerchantContract> GetAsync(int id)
    {
        var contract = await applicationDbContext.TenantMerchantContracts
            .Include(x => x.Tenant)
            .Include(x => x.Merchant)
            .FirstOrDefaultAsync(x => x.Id == id);

        return contract;
    }

    public async Task<bool> IsContractBelongToTenantAsync(int id, int tenantId)
    {
        var result = await applicationDbContext.TenantMerchantContracts.AnyAsync(x => x.Id == id && x.TenantId == tenantId);

        return result;
    }

    public async Task<bool> IsExistsActiveContractAsync(int tenantId, int merchantId, int? contractId = null)
    {
        return await applicationDbContext.TenantMerchantContracts.AnyAsync(x =>
            x.TenantId == tenantId && x.MerchantId == merchantId && x.Status == true && x.EndDate >= System.DateTime.Now && (!contractId.HasValue || x.Id != contractId));
    }

    public async Task<bool> IsDuplicatedContractNumberAsync(string contractNumber)
    {
        return await applicationDbContext.TenantMerchantContracts.AnyAsync(x =>
            x.ContractNumber == contractNumber.Trim() && !x.IsDeleted);
    }

    public async Task<TenantMerchantContract> GetActiveContractAsync(int tenantId, int merchantId)
    {
        //TODO  x.EndDate >= DateTime.Now && DateTime.Now >= x.StartDate 

        return await applicationDbContext.TenantMerchantContracts
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.MerchantId == merchantId && x.Status == true && x.EndDate >= DateTime.Now);
    }

    public async Task<bool> HasEndorsement(int contractId, int tenantId)
    {
        return await applicationDbContext.TenantMerchantContracts.AnyAsync(x => x.ParentId == contractId && x.TenantId == tenantId);
    }

    public async Task<List<int>> GetContractIdsHasEndorsement(List<int> contractIds)
    {
        var result = await applicationDbContext.TenantMerchantContracts
        .Where(x => contractIds.Contains(x.ParentId.Value))
        .Select(x => x.ParentId.Value)
        .ToListAsync();

        return result;
    }

    public async Task<List<ContractGroup>> GetAllGroupContractAsync(CancellationToken cancellationToken)
    {
        return await applicationDbContext.TenantMerchantContracts.GroupBy(p => new ContractIdentifier()
        {
            TenantId = p.TenantId,
            MerchantId = p.MerchantId,
            BillingPeriod = p.BillingPeriod,
            BillingPeriodType = p.BillingPeriodType,
            BillingDailyOriginDate = p.DailyBillingOriginDate
        })
        .Select(p => new ContractGroup()
        {
            TenantId = p.Key.TenantId,
            MerchantId = p.Key.MerchantId,
            BillingPeriod = p.Key.BillingPeriod,
            BillingPeriodType = p.Key.BillingPeriodType,
            BillingDailyOriginDate = p.Key.BillingDailyOriginDate,
            ContractIds = p.OrderByDescending(q => q.CreatedDateTime).Select(q => q.Id),
            EndorsementDate = p.OrderBy(q => q.CreatedDateTime).FirstOrDefault().CreatedDateTime,
            HasEndorsement = p.OrderByDescending(q => q.CreatedDateTime).FirstOrDefault().Children.Any()
        })
        .AsNoTracking()
        .ToListAsync(cancellationToken);
    }

    public async Task<List<ContractGroup>> GetCurrentGroupContractsAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.Today;

        var pc = new PersianCalendar();

        var dailyIds = new List<int>();

        var year = pc.GetYear(today);
        var month = pc.GetMonth(today);
        var day = pc.GetDayOfMonth(today);
        var dayOfWeek = pc.GetDayOfWeek(today);
        var daysInMonth = pc.GetDaysInMonth(year, month);

        var dailyPeriods = await applicationDbContext.TenantMerchantContracts
            .Where(p => p.BillingPeriodType == TimeInterval.Day)
            .Select(p => new
            {
                p.Id,
                p.BillingPeriod,
                p.DailyBillingOriginDate
            })
            .DistinctBy(p => new { p.BillingPeriod, p.DailyBillingOriginDate })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        foreach (var dailyPeriod in dailyPeriods)
        {
            if (!dailyPeriod.DailyBillingOriginDate.HasValue) continue;

            var originDate = dailyPeriod.DailyBillingOriginDate.Value;

            if (today.Date < originDate.Date) continue;

            var difference = (today.Date - originDate.Date).Days;

            if (difference % dailyPeriod.BillingPeriod == 0)
            {
                dailyIds.Add(dailyPeriod.Id);
            }
        }

        return await applicationDbContext.TenantMerchantContracts.GroupBy(p => new ContractIdentifier()
        {
            TenantId = p.TenantId,
            MerchantId = p.MerchantId,
            BillingPeriod = p.BillingPeriod,
            BillingPeriodType = p.BillingPeriodType,
            BillingDailyOriginDate = p.DailyBillingOriginDate
        })
        .Where(p =>
            (p.Key.BillingPeriodType == TimeInterval.Day && p.Select(q => q.Id).Any(q => dailyIds.Contains(q))) ||
            (p.Key.BillingPeriodType == TimeInterval.Month && p.Key.BillingPeriod == day) ||
            (p.Key.BillingPeriodType == TimeInterval.Month && p.Key.BillingPeriod >= 30 && daysInMonth == 29 && day == 29) ||
            (p.Key.BillingPeriodType == TimeInterval.Month && p.Key.BillingPeriod == 31 && daysInMonth == 30 && day == 30) ||
            (p.Key.BillingPeriodType == TimeInterval.Week && p.Key.BillingPeriod == (int)dayOfWeek))
        .Select(p => new ContractGroup()
        {
            TenantId = p.Key.TenantId,
            MerchantId = p.Key.MerchantId,
            BillingPeriod = p.Key.BillingPeriod,
            BillingPeriodType = p.Key.BillingPeriodType,
            BillingDailyOriginDate = p.Key.BillingDailyOriginDate,
            ContractIds = p.OrderByDescending(q => q.CreatedDateTime).Select(q => q.Id),
            EndorsementDate = p.OrderBy(q => q.CreatedDateTime).FirstOrDefault().CreatedDateTime,
            HasEndorsement = p.OrderByDescending(q => q.CreatedDateTime).FirstOrDefault().Children.Any()
        })
        .AsNoTracking()
        .ToListAsync(cancellationToken);
    }
}