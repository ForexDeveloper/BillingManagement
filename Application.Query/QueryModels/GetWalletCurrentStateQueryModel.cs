using Domain.Core.Enums;
using Domain.Core.Helper;
using System;

namespace Application.Query.QueryModels;

public class GetWalletCurrentStateQueryModel
{
    public int AccountId { get; set; }
    public decimal Balance { get; set; }
    public bool HasOverdue { get; set; }
    public bool HasPending { get; set; }
    public WalletType Type { get; set; }
    public string Title { get; set; }
    public WalletStatus Status { get; set; }
    public bool IsDefault { get; set; }
    public int PlanId { get; set; }
    public string TermsAndConditions { get; set; }

}
