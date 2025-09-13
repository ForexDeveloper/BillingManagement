using Domain.Core.AggregateRoots.CategoryAggregate;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repository.EfCore.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public CategoryRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task AddAsync(Category merchant)
        {
            await _applicationDbContext.Categories.AddAsync(merchant);
        }

        public async Task<Category> GetAsync(int id)
        {
            return await _applicationDbContext.Categories.FirstOrDefaultAsync(p => p.Id == id );
        }

        public void Update(Category merchant)
        {
            _applicationDbContext.Categories.Update(merchant);
        }
        public async Task<bool> CheckCategoryAsync(List<int> keys)
       => await _applicationDbContext.Categories.AnyAsync(x => keys.Contains(x.Id));
    }

}
