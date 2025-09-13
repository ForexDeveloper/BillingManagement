using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Customers;
using Domain.Core.Entities.TransactionAggregate.Exceptions;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries
{
    public class GetCustomerWalletTransactionDetailQuery : IRequest<GetCustomerWalletTransactionDetailVm>
    {
        public GetCustomerWalletTransactionDetailQuery(int id, int walletId, long transactionId, int? tenantId = null)
        {
            Id = id;
            WalletId = walletId;
            TransactionId = transactionId;
            TenantId = tenantId;
        }

        public int Id { get; set; }
        public int? TenantId { get; set; }
        public int WalletId { get; set; }
        public long TransactionId { get; }
    }

    public class GetCustomerWalletTransactionDetailQueryhandler : BaseQueryHandler, IRequestHandler<GetCustomerWalletTransactionDetailQuery, GetCustomerWalletTransactionDetailVm>
    {
        private readonly IWalletReadOnlyRepository _walletReadOnlyRepository;

        public GetCustomerWalletTransactionDetailQueryhandler(IWalletReadOnlyRepository walletReadOnlyRepository)
        {
            _walletReadOnlyRepository = walletReadOnlyRepository;
        }

        public async Task<GetCustomerWalletTransactionDetailVm> Handle(GetCustomerWalletTransactionDetailQuery request, CancellationToken cancellationToken)
        {
            var result = await _walletReadOnlyRepository.GetCustomerWalletTransactionDetailAsync(request, cancellationToken);

            if (result == null)
                throw new TransactionNotFoundException("تراکنش یافت نشد.");

            return new GetCustomerWalletTransactionDetailVm()
            {
                Amount = result.Amount,
                Time = result.TransactionTime,
                Type = result.Type,
                TransactionTime = result.TransactionTime,
                TypeTitle = result.TypeTitle,
                Description = result.Description,
                FromWalletName = result.FromWalletName,
                ToWalletName = result.ToWalletName,
                PurchaseTransactionDetail = result.PurchaseTransactionDetail != null ? new PurchaseTransactionDetail
                {
                    FinancialDocumentId = result.PurchaseTransactionDetail.FinancialDocumentId,
                    TrackingCode = result.PurchaseTransactionDetail.TrackingCode,
                    WalletName = result.PurchaseTransactionDetail.WalletName,
                } : null,
                WalletChargeTransactionDetail = result.WalletChargeTransactionDetail != null ? new WalletChargeTransactionDetail
                {
                    WalletName = result.WalletChargeTransactionDetail.WalletName,
                    TrackingCode = result.WalletChargeTransactionDetail.TrackingCode,
                } : null,
                OperationalFeeTransactionDetail = result.OperationalFeeTransactionDetail != null ? new OperationalFeeTransactionDetail
                {
                    OperationalFeeType = result.OperationalFeeTransactionDetail.OperationalFeeType,
                    TrackingCode = result.OperationalFeeTransactionDetail.TrackingCode,
                    WalletName = result.OperationalFeeTransactionDetail.WalletName
                } : null,

                BillingTransactionDetail = result.BillingTransactionDetail != null ? new BaseTransactionDetail
                {
                    WalletName = result.BillingTransactionDetail.WalletName,
                } : null,
                VerificationFeeTransactionDetail = result.VerificationFeeTransactionDetail != null ? new BaseTransactionDetail
                {
                    WalletName = result.VerificationFeeTransactionDetail.WalletName,
                } : null,
                ReverseTransactionDetail = result.ReverseTransactionDetail != null ? new ReverseTransactionDetail
                {
                    WalletName = result.ReverseTransactionDetail.WalletName,
                    FinancialDocumentId = result.ReverseTransactionDetail.FinancialDocumentId
                } : null,
                RefundTransactionDetail = result.RefundTransactionDetail != null ? new ReverseTransactionDetail
                {
                    WalletName = result.RefundTransactionDetail.WalletName,
                    FinancialDocumentId = result.RefundTransactionDetail.FinancialDocumentId
                } : null,
                WithdrawalTransactionDetail = result.WithdrawalTransactionDetail != null ? new BaseTransactionDetail
                {
                    WalletName = result.WithdrawalTransactionDetail.WalletName,
                } : null,
            };
        }
    }
}
