using Domain.Core.Entities.BillingAggregate;
using Domain.Core.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repository.EfCore.Repositories;

public class BillingRepository : IBillingRepository
{
    private readonly ApplicationDbContext _applicationDbContext;

    public BillingRepository(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    public async Task<Billing> GetByIdAsync(long id)
    {
        return await _applicationDbContext.Billings
            .Include(b => b.FromAccount)
            .Include(b => b.BillingPayments)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task AddAsync(Billing billing)
    {
        await _applicationDbContext.Billings.AddAsync(billing);
    }

    public async Task AddRangeAsync(List<Billing> billings)
    {
        await _applicationDbContext.Billings.AddRangeAsync(billings);
    }

    public void Update(Billing billing)
    {
        _applicationDbContext.Billings.Update(billing);
    }

    public void UpdateRange(List<Billing> billings)
    {
        _applicationDbContext.Billings.UpdateRange(billings);
    }

    public async Task<List<long>> GetInstallmentIdsAsync(long billingId)
    {
        var installmentIds = await _applicationDbContext.BillingInstallments
            .Where(x => x.BillingId == billingId)
            .OrderBy(x => x.BillingId)
            .Select(x => x.InstallmentId)
            .ToListAsync();

        return installmentIds;
    }

    public async Task<bool> HasNotCalculatedPenalty(long billingId)
    {
        var result = await (
            from b in _applicationDbContext.Billings
            join bi in _applicationDbContext.BillingInstallments on b.Id equals bi.BillingId
            join i in _applicationDbContext.Installments on bi.InstallmentId equals i.Id
            where b.Id == billingId && i.DueDate < DateTime.Today.AddDays(-i.GracePeriod) && i.State != InstallmentState.CompletePaid
                    && (i.Type == InstallmentType.Installment || i.Type == InstallmentType.Interest)
                   && ((i.LastPenaltyCalculationDate == null || EF.Functions.DateDiffDay(i.LastPenaltyCalculationDate.Value.Date, DateTime.Today) > 0))
            select b
        ).AnyAsync();

        return result;
    }

    public async Task<Billing> GetCurrentBillingByAccountIdAsync(int accountId)
    {
        var currentBilling = await _applicationDbContext.Billings
            .Include(x => x.BillingInstallments)
            .Where(x => x.FromAccountId == accountId &&
                x.StartDate <= DateTime.Today && x.EndDate.AddDays(x.GracePeriod) >= DateTime.Today)
            .OrderBy(x => x.Id).FirstOrDefaultAsync();

        return currentBilling;
    }

    public async Task<Billing> GetLastBillingByAccountIdAsync(int accountId)
    {
        var lastBilling = await _applicationDbContext.Billings
            .Where(x => x.FromAccountId == accountId)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync();

        return lastBilling;
    }

    public async Task<Billing> GetPreviousNotCompletePaidBillingAsync(int accountId, long billingId)
    {
        //TODO
        var previousNotCompletePaidBilling = await _applicationDbContext.Billings
            .Include(x => x.BillingInstallments).ThenInclude(x => x.Installment)
            .Where(x => x.FromAccountId == accountId
                && x.Id < billingId
                //&& x.EndDate < DateTime.Now 
                && x.State != BillingState.CompletePaid
                && x.State != BillingState.Overdue)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync();

        return previousNotCompletePaidBilling;
    }

    public async Task<bool> IsExistPreviousNotCompletePaidBillingAsync(int accountId, long billingId)
    {
        var isExistPreviousNotCompletePaidBilling = await _applicationDbContext.Billings
            .Where(x => x.FromAccountId == accountId
                && x.Id < billingId
                && x.State != BillingState.CompletePaid
                && x.State != BillingState.Overdue)
            .AnyAsync();

        return isExistPreviousNotCompletePaidBilling;
    }

    public async Task<bool> IsLastBilling(int accountId, long billingId)
    {
        var hasNextBilling = await _applicationDbContext.Billings
            .AnyAsync(x => x.FromAccountId == accountId && x.Id > billingId);

        return !hasNextBilling;
    }

    public async Task<List<Billing>> GetAllAsync()
    {
        return await _applicationDbContext.Billings.ToListAsync();
    }

    public async Task<List<Billing>> GetBillingsByAccountIdAsync(int accountId)
    {
        var billings = await _applicationDbContext.Billings
            .Where(x => x.FromAccountId == accountId).ToListAsync();

        return billings;
    }

    public async Task<Billing> GetBillingByInstallmentIdAsync(long installmentId, int tenantId)
    {
        var billingInstallments = await _applicationDbContext.BillingInstallments
            .Include(x => x.Billing)
            .ThenInclude(x => x.FromAccount)
            .Where(x => x.Billing.TenantId == tenantId && x.InstallmentId == installmentId).ToListAsync();

        return billingInstallments.FirstOrDefault().Billing;
    }
}
