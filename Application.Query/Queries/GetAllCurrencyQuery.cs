using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Currencies;
using Domain.Core.AggregateRoots.WalletConfigurationAggregate;
using Domain.Core.AggregateRoots.WalletConfigurationAggregate.Exceptions;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries
{
    public class GetAllCurrencyQuery : IRequest<List<CurrencyViewModel>>
    {

    }
    public class CurrencyQueryHandler : BaseQueryHandler, IRequestHandler<GetAllCurrencyQuery, List<CurrencyViewModel>>
    {
        private readonly ICurrencyReadOnlyRepository _currencyReadOnlyRepository;
        public CurrencyQueryHandler(ICurrencyReadOnlyRepository currencyReadOnlyRepository)
        {
            _currencyReadOnlyRepository = currencyReadOnlyRepository;
        }

        public async Task<List<CurrencyViewModel>> Handle(GetAllCurrencyQuery request, CancellationToken cancellationToken)
        {
            var result = await _currencyReadOnlyRepository.GetAllAsync();

            return result.Select(c => new CurrencyViewModel
            {
                Id = c.Id,
                Title = c.Title,
            }).ToList();
        }
    }
}