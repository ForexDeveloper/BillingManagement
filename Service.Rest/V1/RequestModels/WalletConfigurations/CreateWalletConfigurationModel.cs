using Application.Command.WalletConfigurationCommands;
using Domain.Core.Enums;

namespace Service.Rest.V1.RequestModels.WalletConfigurations
{
    public class CreateWalletConfigurationModel : CreateWalletConfigurationBaseModel
    {
        public int TenantId { get; set; }

    }
    public class CreateWalletConfigurationBaseModel
    {
        public string Title { get; set; }
        public WalletType WalletTypeId { get; set; }
        public decimal MaxWallet { get; set; }
        public decimal? MaximumTotalCredit { get; set; }
        public int? ProjectManagerId { get; set; }
        public int? CurrencyTypeId { get; set; }
        public CreateInstallmentsDto Installments { get; set; }
        public CreateFinancialCommitmentDto Financial { get; set; }

    }
}