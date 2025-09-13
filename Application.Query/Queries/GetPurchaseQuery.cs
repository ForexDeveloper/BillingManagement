using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Tenants;
using MediatR;
using System.Threading.Tasks;
using System.Threading;

namespace Application.Query.Queries;

public class GetPurchaseQuery: IRequest<GetPurchaseVm>
{
    public long FinancialDocumentId { get; set; }
}

public class GetPurchaseQueryHandler : IRequestHandler<GetPurchaseQuery, GetPurchaseVm>
{
    private readonly IFinancialDocumentReadOnlyRepository _repository;

    public GetPurchaseQueryHandler(IFinancialDocumentReadOnlyRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetPurchaseVm> Handle(
        GetPurchaseQuery request,
        CancellationToken cancellationToken)
    {
        var purchases = await _repository.GetPurchaseAsync(
            request.FinancialDocumentId,
            cancellationToken);

        return purchases;
    }
}
