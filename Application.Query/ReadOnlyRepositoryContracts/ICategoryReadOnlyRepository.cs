using Application.Query.QueryModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Query.ReadOnlyRepositoryContracts;

public interface ICategoryReadOnlyRepository
{
    Task<List<CategoryListQueryModel>> GetListAsync();

}
