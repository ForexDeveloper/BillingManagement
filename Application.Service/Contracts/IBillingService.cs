using System.Threading.Tasks;

namespace Application.Service.Contracts;

public interface IBillingService
{
    Task CreateLoanWalletBillings();
    Task UpdateBillings();
}
