using Domain.Core.Entities.AccountAggregate;
using Domain.Core.Enums;

namespace Domain.Core.Entities.WalletAggregate
{
    public class LoanWallet : Wallet
    {
        #region Property

        public decimal OperationalFee { get; private set; }

        public OperationalFeeType OperationalFeeType { get; private set; }

        public decimal InitialAmount { get; private set; }

        public int NumberOfInstallment { get; private set; }

        public long? UserCreditGrantingProcessId { get; private set; }
        public WalletSettlementType SettlementType { get; private set; }

        #endregion Property

        private LoanWallet() { }

        public LoanWallet(int businessIdentityId, int tenantId, Account account, int planId,
            decimal operationalFee, OperationalFeeType operationalFeeType, decimal initialAmount, int numberOfInstallment,
            int walletContractId, long? userCreditGrantingProcessId = null, WalletSettlementType settlementType = WalletSettlementType.Cash)
            : base(businessIdentityId, tenantId, account, planId, walletContractId)
        {
            OperationalFee = operationalFee;
            OperationalFeeType = operationalFeeType;
            InitialAmount = initialAmount;
            NumberOfInstallment = numberOfInstallment;
            UserCreditGrantingProcessId = userCreditGrantingProcessId;
            SettlementType = settlementType;
        }
    }
}