using Domain.Core.Enums;

namespace Application.Query.QueryModels;

public class CashWalletQueryModel
{
    public int Id { get; set; }
    public decimal Balance { get; set; }
    public decimal NonWithDrawableBalance { get; set; }
    public decimal WithDrawableBalance { get; set; }
    public AccountStatus Status { get; set; }
    public string TermsAndConditions { get; set; }
}