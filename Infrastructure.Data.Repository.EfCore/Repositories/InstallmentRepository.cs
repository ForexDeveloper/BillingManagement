using Domain.Core.Entities.InstallmentAggregate;
using Domain.Core.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repository.EfCore.Repositories;

public class InstallmentRepository : IInstallmentRepository
{
    private readonly ApplicationDbContext _applicationDbContext;

    public InstallmentRepository(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    public async Task AddAsync(Installment installment)
    {
        await _applicationDbContext.Installments.AddAsync(installment);
    }

    public async Task AddRangeAsync(List<Installment> installments)
    {
        await _applicationDbContext.Installments.AddRangeAsync(installments);
    }

    public void Update(Installment installment)
    {
        _applicationDbContext.Installments.Update(installment);
    }

    public async Task ExecuteUpdateAsync(List<long> installmentIds)
    {
        await _applicationDbContext.Installments
            .Where(i => installmentIds.Contains(i.Id))
            .ExecuteUpdateAsync(updates =>
                updates.SetProperty(i => i.HasBilling, true)
                       .SetProperty(i => i.EditDateTime, DateTime.Now)
            );
    }

    public void UpdateRange(List<Installment> installments)
    {
        _applicationDbContext.Installments.UpdateRange(installments);
    }

    public async Task<List<int>> GetAccountIdsForCreateOrUpdateBilling()
    {
        //todo 
        var accountIds = await _applicationDbContext.Installments.AsNoTracking()
        .Where(x => x.HasBilling && x.DueDate < DateTime.Today.AddDays(-x.GracePeriod) && x.State != InstallmentState.CompletePaid
                     && (x.Type == InstallmentType.Installment || x.Type == InstallmentType.Interest)
                     && (x.LastPenaltyCalculationDate == null || EF.Functions.DateDiffDay(x.LastPenaltyCalculationDate.Value.Date, DateTime.Today) > 0
                     && x.SettlementType == WalletSettlementType.Cash)
            )
        .OrderBy(g => g.StartDate)
        .Select(x => x.FromAccountId)
        .Distinct()
        .Take(500)
        .ToListAsync();

        return accountIds;
    }

    public async Task<List<int>> GetAccountIdsForCreateBilling()
    {
        var accountIds = await _applicationDbContext.Installments.AsNoTracking()
        .Where(x => x.Type == InstallmentType.Installment && !x.HasBilling)
        .OrderBy(g => g.StartDate)
        .Select(x => x.FromAccountId)
        .Distinct()
        .Take(500)
        .ToListAsync();

        return accountIds;
    }

    public async Task<Installment> GetById(long id)
    {
        var installment = await _applicationDbContext.Installments
            .FirstOrDefaultAsync(t => t.Id == id);
        return installment;
    }

    public async Task<List<Installment>> GetByIds(List<long> installmentIds)
    {
        var result = await _applicationDbContext.Installments
            .Where(t => installmentIds.Contains(t.Id) ||
                (t.ParentId != null && installmentIds.Contains(t.ParentId.Value)))
            .ToListAsync();

        return result;
    }

    public async Task<List<Installment>> GetNotCompletePaidInstallmentsByAccountId(int accountId)
    {
        var installmentIds = await _applicationDbContext.Installments
            .Where(x => x.FromAccountId == accountId && x.HasBilling
                && x.DueDate < DateTime.Today.AddDays(-x.GracePeriod) && x.State != InstallmentState.CompletePaid
                && (x.Type == InstallmentType.Installment || x.Type == InstallmentType.Interest))
            .OrderBy(x => x.Id)
            .Select(x => x.ParentId ?? x.Id)
            .Distinct()
            .ToListAsync();

        var result = await _applicationDbContext.Installments
            .Where(t => installmentIds.Contains(t.Id) ||
                (t.ParentId != null && installmentIds.Contains(t.ParentId.Value) && t.Type == InstallmentType.Interest))
            .ToListAsync();

        return result;
    }

    public async Task<List<long>> GetParentInstallmentsWithoutBilling()
    {
        var installmentIds = await _applicationDbContext.Installments
                .Where(x => x.Type == InstallmentType.Installment && !x.HasBilling)
                .OrderBy(g => g.StartDate)
                .Select(x => x.Id)
                .Take(100)
                .ToListAsync();

        return installmentIds;
    }

    public async Task<List<Installment>> GetInstallmentsByIds(List<long> installmentIds)
    {
        var result = await _applicationDbContext.Installments
            .Include(x => x.FromAccount)
            .Where(t => installmentIds.Contains(t.Id) ||
                        (t.ParentId != null && installmentIds.Contains(t.ParentId.Value) && t.Type == InstallmentType.Interest))
            .ToListAsync();

        return result;
    }

    public async Task<List<Installment>> GetAllAsync()
    {
        return await _applicationDbContext.Installments.ToListAsync();
    }

    public async Task<List<Installment>> GetInstallmentsByAccountId(int accountId)
    {
        var result = await _applicationDbContext.Installments
            .Include(x => x.FromAccount)
            .Where(t => t.FromAccountId == accountId)
            .ToListAsync();

        return result;
    }
}
