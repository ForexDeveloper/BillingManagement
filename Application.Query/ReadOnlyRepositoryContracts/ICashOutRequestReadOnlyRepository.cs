using Application.Query.Queries;
using Application.Query.QueryModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Query.ReadOnlyRepositoryContracts;

public interface ICashOutRequestReadOnlyRepository
{
    Task<CustomerCashOutRequestsQueryModel> GetCustomerCashOutRequestListAsync(GetCustomersCashOutRequestsQuery query);
}