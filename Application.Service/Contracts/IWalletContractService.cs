using Domain.Core.Entities.WalletContractAggregate;
using Shared.EventBus.Events;
using System.Collections.Generic;

namespace Application.Service.Contracts;
public interface IWalletContractService
{
    void SetWalletContractGuarantor(WalletContract contract, List<FcmWalletContractGuarantor> guarantors);
    void SetWalletContractFinancier(WalletContract contract, List<FcmWalletContractFinancier> financiers);
    void SetWalletContractFacilitators(WalletContract contract, List<FcmWalletContractFacilitator> facilitators);
}
