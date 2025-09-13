using Application.Query.ViewModels.Customers;
using Domain.Core.Enums;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.QueryModels;

namespace Application.Query.Queries
{
    public class GetCustomerBillPaymentDetailQuery : IRequest<GetCustomerBillPaymentDetailVm>
    {
       public int CustomerId { get; set; }
       public long BillId { get; set; }
       public int? TenantId{ get; set; }
       public int PaymentId{ get; set; }
        public GetCustomerBillPaymentDetailQuery(int customerId ,long billId , int? tenantId,int paymentId)
        {
            CustomerId = customerId;
            BillId = billId;
            TenantId = tenantId;
            PaymentId = paymentId;
        }
    }

    public class GetCustomerBillPaymentDetailQueryHandler : IRequestHandler<GetCustomerBillPaymentDetailQuery, GetCustomerBillPaymentDetailVm>
    {
        private readonly IBillingReadOnlyRepository _billingReadOnlyRepository;
        public GetCustomerBillPaymentDetailQueryHandler(IBillingReadOnlyRepository billingReadOnlyRepository)
        {
            _billingReadOnlyRepository = billingReadOnlyRepository;
        }

        public async Task<GetCustomerBillPaymentDetailVm> Handle(GetCustomerBillPaymentDetailQuery request, CancellationToken cancellationToken)
        {
            GetCustomerBillPaymentDetailQueryModel result =
                _billingReadOnlyRepository.GetCustomerBillPaymentDetailAsync(request.CustomerId, request.BillId,
                    request.TenantId,request.PaymentId);
            return result.MapToViewModel();
        }
    }
}