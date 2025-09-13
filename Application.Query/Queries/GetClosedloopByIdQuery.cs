using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Closedloops;
using Domain.Core.Entities.ClosedloopAggregate.Exceptions;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries;

public class GetClosedloopByIdQuery : IRequest<ClosedloopViewModel>
{
    public int Id { get; }
    public int? TenantId { get; }

    public GetClosedloopByIdQuery(int id, int? tenantId = null)
    {
        Id = id;
        TenantId = tenantId;
    }
}

public class ClosedloopGetByIdQueryHandler : BaseQueryHandler, IRequestHandler<GetClosedloopByIdQuery, ClosedloopViewModel>
{
    private readonly IClosedloopReadOnlyRepository _closedloopReadOnlyRepository;
    public ClosedloopGetByIdQueryHandler(IClosedloopReadOnlyRepository closedloopReadOnlyRepository)
    {
        _closedloopReadOnlyRepository = closedloopReadOnlyRepository;
    }

    public async Task<ClosedloopViewModel> Handle(GetClosedloopByIdQuery request, CancellationToken cancellationToken)
    {
        var result = await _closedloopReadOnlyRepository.GetByIdAsync(request.Id);
        if (result == null)
            throw new ClosedloopNotFoundException("شناسه بدرستی ارسال نشده.");


        return new ClosedloopViewModel
        {
            Categories = result.Categories.Select(c => new ClosedloopCategoryViewModel
            {
                CategorId = c.CategorId,
                CategorIdTitle = c.CategorIdTitle,
                Id = c.Id
            }).ToList(),
            TenantTitle = result.TenantTitle,
            WalletConfigurationTitle = result.WalletConfigurationTitle,
            TenantId = result.TenantId,
            WalletConfigurationId = result.WalletConfigurationId,
            Title = result.Title,
            Id = result.Id,
            Merchants = result.Merchants.Select(c => new ClosedloopMerchantViewModel
            {
                Id = c.Id,
                MerchantId = c.MerchantId,
                MerchantTitle = c.MerchantTitle
            }).ToList(),
        };
    }
}

