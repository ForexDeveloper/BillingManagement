using Domain.Core.Enums;
using System;
using System.Collections.Generic;

namespace Application.Query.QueryModels;

public class WalletContractQueryModel
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string TenantName { get; set; }
    public int? ParentId { get; set; }
    public string ContractNumber { get; set; }
    public WalletContractStatus Status { get; set; }
    public string StatusTitle { get; set; }
    public List<WalletContractPlanQueryModel> Plans { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationTitle { get; set; }
    public int? GrantingProcessId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime EditDateTime { get; set; }
    public WalletContractStatus? LastEndorsementStatus { get; set; }
}