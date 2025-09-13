using Application.Query.Queries;
using Application.Query.QueryModels;
using Application.Query.ViewModels.Wallets;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.ReadOnlyRepositoryContracts
{
    public interface IBillingReadOnlyRepository
    {
        Task<GetCustomerWalletViewModel> GetLastUnpaidBillOfCurrentMonth(int accountId, CancellationToken cancellationToken);
        Task<GetCustomerBillDetailsQueryModel> GetCustomerBillDetailsAsync(GetCustomerBillDetailsQuery query, CancellationToken cancellationToken);
        GetCustomerBillPaymentDetailQueryModel GetCustomerBillPaymentDetailAsync(int customerId, long billId, int? tenantId, int paymentId);
        Task<BillsQueryModel> GetCustomerBillsAsync(GetCustomerBillsQuery query, CancellationToken cancellationToken);
        Task<GetCustomerIdAndBillRemainAmountQueryModel> GetCustomerIdAndBillAmountsByIdAsync(int tenantId, long id);
    }
}