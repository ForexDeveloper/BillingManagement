using System;
using System.Linq;
using System.Threading;
using Domain.Core.Enums;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;
using Application.Service.Helper;
using Microsoft.EntityFrameworkCore;
using Domain.Core.Entities.TenantMerchantContractAggregate;
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;
using Domain.Core.Entities.TenantMerchantContractAggregate.Dtos;

namespace Infrastructure.Data.Repository.EfCore.Repositories;

public sealed class TenantMerchantContractRepository(ApplicationDbContext applicationDbContext) : ITenantMerchantContractRepository
{
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

        return await applicationDbContext.TenantMerchantContracts.OrderByDescending(p => p.CreatedDateTime)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.MerchantId == merchantId && x.Status);

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
        var contracts = await applicationDbContext.TenantMerchantContracts.AsNoTracking().ToListAsync(cancellationToken);

        return contracts.GroupBy(p => new ContractIdentifier()
        {
            TenantId = p.TenantId,
            MerchantId = p.MerchantId,
            BillingPeriod = p.BillingPeriod,
            BillingPeriodType = p.BillingPeriodType,
            CommissionCalculationType = p.CommissionCalculationType,
            DailyBillingOriginDate = p.DailyBillingOriginDate?.Date
        })
        .Select(p => new ContractGroup()
        {
            TenantId = p.Key.TenantId,
            MerchantId = p.Key.MerchantId,
            BillingPeriod = p.Key.BillingPeriod,
            BillingPeriodType = p.Key.BillingPeriodType,
            DailyBillingOriginDate = p.Key.DailyBillingOriginDate,
            CommissionCalculationType = p.Key.CommissionCalculationType,
            Status = p.OrderByDescending(q => q.CreatedDateTime).First().Status,
            CreatedDateTime = p.OrderBy(q => q.CreatedDateTime).First().CreatedDateTime,
            EndorsementDate = p.OrderByDescending(q => q.CreatedDateTime).First().EditDateTime,
            ContractIds = p.OrderByDescending(q => q.CreatedDateTime).Select(q => q.Id).ToList(),

            TieredCommissions = p.Any(q => q.Status)
                 ? p.First(q => q.Status).TieredCommissions
                 : p.OrderByDescending(q => q.CreatedDateTime).First().TieredCommissions,

            CommissionReferenceTypes = p.Any(q => q.Status)
                 ? p.First(q => q.Status).CommissionReferenceTypes
                 : p.OrderByDescending(q => q.CreatedDateTime).First().CommissionReferenceTypes,

            PeriodMinCommissionAmount = p.Any(q => q.Status)
                 ? p.First(q => q.Status).PeriodMinCommissionAmount
                 : p.OrderByDescending(q => q.CreatedDateTime).First().PeriodMinCommissionAmount,

            PeriodMaxCommissionAmount = p.Any(q => q.Status)
                 ? p.First(q => q.Status).PeriodMaxCommissionAmount
                 : p.OrderByDescending(q => q.CreatedDateTime).First().PeriodMaxCommissionAmount
        })
        .OrderBy(p => p.Status)
        .ToList();
    }

    public DateTime GetActiveContractStartOfPeriod(TenantMerchantContract contract)
    {
        int difference;
        DateTime endOfPeriod;
        DateTime startOfPeriod;

        var today = DateTime.Today;

        var pc = new PersianCalendar();

        var year = pc.GetYear(today);
        var month = pc.GetMonth(today);
        var dayOfWeek = pc.GetDayOfWeek(today);
        var dayOfMonth = pc.GetDayOfMonth(today);

        var billingPeriod = contract.BillingPeriod;

        switch (contract.BillingPeriodType)
        {
            case TimeInterval.Day:

                if (!contract.DailyBillingOriginDate.HasValue) throw new Exception();

                var originDate = contract.DailyBillingOriginDate.Value;

                if (today.Date < originDate.Date) throw new Exception();

                var totalDays = (today.Date - originDate.Date).Days;

                difference = billingPeriod - (totalDays % billingPeriod);

                endOfPeriod = pc.AddDays(today, difference);

                startOfPeriod = pc.AddDays(endOfPeriod, -billingPeriod);

                break;

            case TimeInterval.Week:

                if ((DayOfWeek)billingPeriod >= dayOfWeek)
                {
                    difference = billingPeriod - (int)dayOfWeek;
                }
                else
                {
                    difference = 7 - ((int)dayOfWeek - billingPeriod);
                }

                endOfPeriod = pc.AddDays(new DateTime(year, month, dayOfMonth, pc), difference);

                startOfPeriod = pc.AddWeeks(endOfPeriod, -1);

                break;

            case TimeInterval.Month:

                billingPeriod = DateHelper.RegulateBillingPeriod(pc, year, month, billingPeriod);

                if (billingPeriod >= dayOfMonth)
                {
                    difference = billingPeriod - dayOfMonth;

                    endOfPeriod = pc.AddMonths(new DateTime(year, month, dayOfMonth, pc), difference);
                }
                else
                {
                    endOfPeriod = pc.AddMonths(new DateTime(year, month, billingPeriod, pc), 1);
                }

                startOfPeriod = pc.AddMonths(endOfPeriod, -1);

                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        return startOfPeriod;
    }
}