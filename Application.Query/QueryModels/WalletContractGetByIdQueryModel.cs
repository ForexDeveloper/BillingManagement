using Domain.Core.Entities.WalletContractAggregate;
using Domain.Core.Enums;
using System;
using System.Collections.Generic;

namespace Application.Query.QueryModels;

public class WalletContractGetByIdQueryModel
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string TenantName { get; set; }
    public string ContractNumber { get; set; }
    public WalletContractStatus Status { get; set; }
    public int PlanId { get; set; }
    public string PlanTitle { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationTitle { get; set; }
    public int? TenantIpgSettingId { get; set; }
    public string TenantIpgSettingName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool AssignWalletToOrganizationCustomers { get; set; } = true;
    public WalletType WalletType { get; set; }
    public int? GrantingProcessId { get; set; }
    public int? RootParentId { get; set; }
    public List<WalletContractRejectionReason>? Rejections { get; set; }
    public List<WalletContractPlanQueryModel> WalletContractPlans { get; set; }
    public List<WalletContractGuarantorQueryModel> WalletContractGuarantors { get; set; }
    public List<WalletContractFinancierQueryModel> WalletContractFinanciers { get; set; }
    public List<WalletContractFacilitatorQueryModel> WalletContractFacilitators { get; set; }
}