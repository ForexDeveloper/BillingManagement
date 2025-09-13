using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Query.QueryModels;

namespace Application.Query.ReadOnlyRepositoryContracts;

public interface IBankAccountReadOnlyRepository
{
    Task<List<GetCustomerBankAccountQueryModel>> GetCustomerBankAccountsByBusinessIdentityIdAsync(int customerId,int? tenantId=null);
}