using Domain.Base;
using Domain.Core.AggregateRoots.CategoryAggregate;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Core.AggregateRoots.CategoryAggregate
{
    public interface ICategoryRepository
    {
        Task AddAsync(Category category);
        void Update(Category category);
        Task<Category> GetAsync(int id );
        Task<bool> CheckCategoryAsync(List<int> keys);
    }
}