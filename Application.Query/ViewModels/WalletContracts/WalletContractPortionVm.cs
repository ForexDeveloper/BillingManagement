using Domain.Core.Enums;

namespace Application.Query.ViewModels.WalletContracts;

public class WalletContractPortionVm
{
    public WalletPortionType PortionType { get; set; }
    public string PortionTypeName { get; set; }

    public WalletContractPortionVm(WalletPortionType portionType, string portionTypeName)
    {
        PortionType = portionType;
        PortionTypeName = portionTypeName;
    }
}