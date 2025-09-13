using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Domain.Core.Entities.MerchantInstallmentAggregate.ValueObjects;

namespace Domain.Core.Entities.MerchantInstallmentAggregate;

public interface IMerchantInstallmentRepository
{
    Task AddRangeAsync(List<MerchantInstallment> installments);

    Task<List<GroupIdentityInstallments>> GetGroupIdentityInstallments(CancellationToken cancellationToken);

    Task<List<GroupIdentityInstallments>> GetGroupIdentityContractInstallments(CancellationToken cancellationToken);

    Task Calculate(CancellationToken cancellationToken);
}