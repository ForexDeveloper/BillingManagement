using Domain.Core.Enums;
using static Application.Command.WalletConfigurationCommands.UpdateWalletConfigurationCommand;

namespace Service.Rest.V1.RequestModels.WalletConfigurations
{
    public class UpdateWalletConfigurationModel
    {
        public string Title { get; set; }
        public WalletType WalletTypeId { get; set; }
        public decimal MaxWallet { get; set; }
        public decimal? MaximumTotalCredit { get; set; }
        public int? ProjectManagerId { get; set; }
        public int? CurrencyTypeId { get; set; }
        public UpdateInstallmentsDto Installments { get; set; }
        public UpdateFinancialCommitmentDto Financial { get; set; }
    }
}