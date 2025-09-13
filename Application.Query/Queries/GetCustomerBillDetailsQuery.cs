using Application.Query.QueryModels;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Customers;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries
{
    public class GetCustomerBillDetailsQuery : IRequest<GetCustomerBillDetailsVm>
    {
        public int CustomerId { get; set; }
        public long BillId { get; set; }
        public int? TenantId { get; set; }
        public int? WalletId { get; set; }

        public GetCustomerBillDetailsQuery(int customerId, long billId, int? walletId, int? tenantId = null)
        {
            CustomerId = customerId;
            BillId = billId;
            TenantId = tenantId;
            WalletId = walletId;
        }


    }
    public class GetCustomerBillDetailsQueryHandler : IRequestHandler<GetCustomerBillDetailsQuery, GetCustomerBillDetailsVm>
    {
        private readonly IBillingReadOnlyRepository _billingRepository;

        private readonly IAttachmentReadOnlyRepository _attachmentRepository;
        public GetCustomerBillDetailsQueryHandler(IBillingReadOnlyRepository billingRepository, IAttachmentReadOnlyRepository attachmentRepository)
        {
            _billingRepository = billingRepository;
            _attachmentRepository = attachmentRepository;
        }

        public async Task<GetCustomerBillDetailsVm> Handle(GetCustomerBillDetailsQuery request, CancellationToken cancellationToken)
        {
            GetCustomerBillDetailsQueryModel result = await _billingRepository.GetCustomerBillDetailsAsync(request, cancellationToken);


            var model = await _attachmentRepository.GetAsync(Domain.Core.Enums.EntityType.Plan, result.PlanId);
            if (model != null)
            {
                result.WalletLogoId = string.IsNullOrWhiteSpace(model.FileReference) ? string.Empty : model.FileReference;
            }

            return result.ToViewModel();
        }
    }
}