using Domain.Core.Enums;
using System;

public class WalletContractEndorsementVm
{
    public int Id { get; set; }
    public string ContractNumber { get; set; }
    public WalletContractStatus Status { get; set; }
    public string StatusTitle { get; set; }
    public DateTime? ChangeStatusDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string DisplayEndDate { get; set; }
}