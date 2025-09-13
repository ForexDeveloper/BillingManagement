using Domain.Core.Enums;
using System;

namespace Application.Query.QueryModels;

public class WalletContractChildQueryModel
{
    public int Id { get; set; }
    public string ContractNumber { get; set; }
    public int? RootParentId { get; set; }
    public WalletContractStatus Status { get; set; }
    public DateTime? ChangeStatusDate { get; set; }
    public DateTime? EndDate { get; set; }
}