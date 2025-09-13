using Domain.Core.Entities.AccountAggregate;
using Domain.Core.Entities.PlanAggregate;
using Domain.Core.Enums;

namespace Application.Service.Dtos.Wallet;

public class CustomerWalletDto
{
    public Plan Plan { get; set; }
    public Account TenantLoanAccount { get; set; }
    public int WalletContractId { get; set; }
    public int CurrencyTypeId { get; set; }
    public OperationalFeeType OperationalFeeType { get; set; }
    public bool IsDefault { get; set; }
}
