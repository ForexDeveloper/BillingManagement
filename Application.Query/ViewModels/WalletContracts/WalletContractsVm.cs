using Application.Service.Enums;
using Domain.Core.Enums;
using System;
using System.Collections.Generic;

namespace Application.Query.ViewModels.WalletContracts;

public class WalletContractsVm
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string TenantName { get; set; }
    public string ContractNumber { get; set; }
    public WalletContractStatus Status { get; set; }
    public string StatusTitle { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationTitle { get; set; }
    public List<WalletContractPlanVm> Plans { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string DisplayEndDate { get; set; }
    public DateTime EditDateTime { get; set; }
    public WalletContractStatus? LastEndorsementStatus { get; set; }
    public string LastEndorsementStatusTitle { get; set; }
    public WalletContractOriginType WalletContractOriginType { get; set; }

}