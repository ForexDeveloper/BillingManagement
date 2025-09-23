using System.Threading.Tasks;
using Application.Query.Queries.Billings;
using Application.Query.ViewModels.Billings;

namespace Application.Query.ReadOnlyRepositoryContracts;

public interface IBillingReadOnlyRepository
{
    Task<GetBillingsViewModel> GetBillingsAsync(GetBillingsQuery query);
}