using Domain.Base;
using Domain.Core.Entities.Providers;
using Domain.Core.Entities.Shared.Exceptions;

namespace Domain.Core.Entities.TenantPlatformContractAggregate;

public class TenantPlatformContractProvider : BaseEntity<int>
{
    public int TenantPlatformContractId { get; private set; }
    public TenantPlatformContract TenantPlatformContract { get; private set; }
    public int ProviderId { get; private set; }
    public Provider Provider { get; private set; }
    public decimal Amount { get; private set; }

    public TenantPlatformContractProvider(int tenantPlatformContractId, int providerId, decimal amount)
    {
        TenantPlatformContractId = tenantPlatformContractId;
        ProviderId = providerId;
        Amount = amount;
    }

    public void SetAmount(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentValidationException(nameof(Amount), "مقدار قیمت صحیح نمی باشد.");
        Amount = amount;
    }
}