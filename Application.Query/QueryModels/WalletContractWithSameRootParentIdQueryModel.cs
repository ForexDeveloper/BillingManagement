using Domain.Core.Enums;
using System.Collections.Generic;

namespace Application.Query.QueryModels;

public class WalletContractWithSameRootParentIdQueryModel
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int? RootParentId { get; set; }
    public WalletContractStatus? Status { get; set; }
    public int OrganizationId { get; set; }
    public List<int> PlanIds { get; set; }
}