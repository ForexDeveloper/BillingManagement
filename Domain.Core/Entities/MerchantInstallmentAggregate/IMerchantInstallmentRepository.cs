using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Domain.Core.Entities.TenantMerchantContractAggregate.Dtos;

namespace Domain.Core.Entities.MerchantInstallmentAggregate;

public interface IMerchantInstallmentRepository
{
    Task AddRangeAsync(List<MerchantInstallment> installments);

    IQueryable<MerchantInstallment> CreateJobInstallmentQuery(DateTime startOfPeriod,
        DateTime endOfPeriod, IEnumerable<int> contractIds);

    Task<bool> FindInContractPeriodAsync(IQueryable<MerchantInstallment> query, CancellationToken cancellationToken);

    Task<InstallmentRange?> GetInstallmentsRanges(IEnumerable<int> contractIds, CancellationToken cancellationToken);

    Task<Dictionary<ContractIdentifier, List<MerchantInstallment>>> GetGroupContractInstallments(IQueryable<MerchantInstallment> query, CancellationToken cancellationToken);
}

public sealed record InstallmentRange(DateTime MinDueDate, DateTime MaxDueDate)
{
    public DateTime MinDueDate { get; set; } = MinDueDate;

    public DateTime MaxDueDate { get; set; } = MaxDueDate;

    public bool HasIntersection(DateTime startOfPeriod, DateTime endOfPeriod)
    {
        return startOfPeriod <= MaxDueDate && MinDueDate < endOfPeriod;
    }
}