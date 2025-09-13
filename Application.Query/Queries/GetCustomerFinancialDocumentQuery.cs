using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Customers;
using MediatR;
using System.Threading.Tasks;
using System.Threading;
using Application.Query.QueryModels;
using Domain.Core.Entities.FinancialDocumentAggregate.Exceptions;

namespace Application.Query.Queries
{
    public class GetCustomerFinancialDocumentQuery : IRequest<CustomerFinancialDocumentDetailQueryModel>
    {
        public GetCustomerFinancialDocumentQuery(int id, long financialDocumentId, int? tenantId = null)
        {
            Id = id;
            TenantId = tenantId;
            FinancialDocumentId = financialDocumentId;
        }

        public int Id { get; set; }
        public int? TenantId { get; set; }
        public long FinancialDocumentId { get; set; }
    }


    public class GetCustomerFinancialDocumentQueryHandler : IRequestHandler<GetCustomerFinancialDocumentQuery, CustomerFinancialDocumentDetailQueryModel>
    {
        private readonly IFinancialDocumentReadOnlyRepository _financialDocumentReadOnlyRepository;

        public GetCustomerFinancialDocumentQueryHandler(IFinancialDocumentReadOnlyRepository financialDocumentReadOnlyRepository)
        {
            _financialDocumentReadOnlyRepository = financialDocumentReadOnlyRepository;
        }

        public async Task<CustomerFinancialDocumentDetailQueryModel> Handle(GetCustomerFinancialDocumentQuery request, CancellationToken cancellationToken)
        {
            var result = await _financialDocumentReadOnlyRepository.GetCustomerFinancialDocumentAsync(request, cancellationToken);
            if (result == null)
            {
                throw new FinancialDocumentNotFoundException("گزارش مالی پیدا نشد");
            }
            return result;
        }
    }
}
