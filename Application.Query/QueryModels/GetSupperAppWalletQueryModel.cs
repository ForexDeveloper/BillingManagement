using Domain.Core.Enums;

namespace Application.Query.QueryModels;

public record GetSuperAppWalletQueryModel
{
    public int Id { get; set; }

    public int PlanId { get; set; }

    public string Title { get; set; }

    public bool IsDefault { get; set; }

    public decimal Balance { get; set; }

    public string OrganizationTitle { get; set; }

    public WalletType WalletType { get; set; }

    public WalletStatus WalletStatus { get; set; }
}