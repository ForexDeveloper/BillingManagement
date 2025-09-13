using Domain.Base;
using Domain.Core.Entities.TenantPlatformContractAggregate;
using Domain.Core.Enums;
using System.Collections.Generic;

namespace Domain.Core.Entities.Providers;

public class Provider : BaseEntity<int>
{
    public ProviderType ProviderType { get; private set; }
    public string Name { get; private set; }
    public string EnglishName { get; private set; }
    public string Description { get; private set; }
    public List<TenantPlatformContractProvider> TenantPlatformContractProviders { get; private set; }
}