using Application.Query.QueryModels;
using Application.Query.ReadOnlyRepositoryContracts;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repository.EfCore.ReadonlyRepositories
{
    public class CategoryReadOnlyRepository : ICategoryReadOnlyRepository
    {
        private readonly ReadonlyApplicationDbContext _readonlyApplicationDbContext;
        public CategoryReadOnlyRepository(ReadonlyApplicationDbContext readonlyApplicationDbContext)
        {
            _readonlyApplicationDbContext = readonlyApplicationDbContext;
        }


        public async Task<List<CategoryListQueryModel>> GetListAsync()
        {
            var result = await _readonlyApplicationDbContext.Categories.Include(c=>c.Categories).ToListAsync();
            var categoryModel = result.Select(c => new CategoryListQueryModel
            {
                Id = c.Id,
                ParentId = c.ParentId,
                Title = c.Title,
                Categories = c.Categories.Select(x => new CategoryListQueryModel
                {
                    Id = x.Id,
                    ParentId = x.ParentId,
                    Title = x.Title,
                }).ToList()
            }).ToList();
            return categoryModel;
        }
    }

}
