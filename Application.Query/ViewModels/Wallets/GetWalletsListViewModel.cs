using Domain.Core.Enums;
using Domain.Core.Helper;

namespace Application.Query.ViewModels.Wallets;


public class GetWalletsListViewModel
{
    public WalletType Type { get; set; }
    public string TypeTitle => Type.GetEnumDescription();
    public int Id { get; set; }
    public string Title { get; set; }
    public string OrganizationTitle { get; set; }
    public decimal Balance { get; set; }
    public string LogoId { get; set; }
    public WalletStatus Status { get; set; }
    public bool IsDefault { get; set; }
    public string StatusTitle => Status.GetEnumDescription();

}
