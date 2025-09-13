using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Customers;
using MediatR;

namespace Application.Query.Queries;

public class GetCustomerBankAccountsQuery:IRequest<List<GetCustomerBankAccountViewModel>>
{
    public GetCustomerBankAccountsQuery(int customerId, int tenantId)
    {
        CustomerId = customerId;
        TenantId = tenantId;
    }

    public int CustomerId { get; set; }
    public int TenantId { get; set; }
}

public class GetCustomerBankAccountsQueryHandler : BaseQueryHandler, IRequestHandler<GetCustomerBankAccountsQuery,List<GetCustomerBankAccountViewModel>>
{
    private readonly IBankAccountReadOnlyRepository _bankAccountReadOnlyRepository;

    public GetCustomerBankAccountsQueryHandler(IBankAccountReadOnlyRepository bankAccountReadOnlyRepository)
    {
        _bankAccountReadOnlyRepository = bankAccountReadOnlyRepository;
    }

    public async Task<List<GetCustomerBankAccountViewModel>> Handle(GetCustomerBankAccountsQuery request, CancellationToken cancellationToken)
    {
        var result = await _bankAccountReadOnlyRepository.GetCustomerBankAccountsByBusinessIdentityIdAsync(request.CustomerId,request.TenantId);

        return result.Select(ba => new GetCustomerBankAccountViewModel(ba)).ToList();
    }
}
