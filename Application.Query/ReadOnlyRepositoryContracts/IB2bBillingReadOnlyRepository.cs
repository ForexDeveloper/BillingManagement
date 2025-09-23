using Application.Query.Queries.Billings;
using Application.Query.ViewModels.Billings;
using System.Threading.Tasks;

namespace Application.Query.ReadOnlyRepositoryContracts;

public interface IB2bBillingReadOnlyRepository
{
    Task<GetBillingsViewModel> GetBillingsAsync(GetBillingsQuery query);
}