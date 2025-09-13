using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Customers;
using Domain.Core.Entities.CustomerAggregate.Exceptions;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries
{
    public class GetCustomerByIdQuery : IRequest<GetCustomerVm>
    {
        public int Id { get; set; }
        public int? TenantId { get; set; }
        public GetCustomerByIdQuery(int id, int? tenantId = null)
        {
            Id = id;
            TenantId = tenantId;
        }
    }

    public class GetCustomerByIdQueryHandler : BaseQueryHandler, IRequestHandler<GetCustomerByIdQuery, GetCustomerVm>
    {
        private readonly ICustomerReadOnlyRepository _customerReadOnlyRepository;
        public GetCustomerByIdQueryHandler(ICustomerReadOnlyRepository customerReadOnlyRepository)
        {
            _customerReadOnlyRepository = customerReadOnlyRepository;
        }

        public async Task<GetCustomerVm> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _customerReadOnlyRepository.GetAsync(request.Id, request.TenantId);
            if (result == null)
                throw new CustomerNotFoundException("شناسه بدرستی ارسال نشده");

            return new GetCustomerVm
            {
                Id = result.Id,
                TenantId = result.TenantId,
                TenantName = result.TenantName,
                FullName = result.FullName
            };

        }
    }
}
