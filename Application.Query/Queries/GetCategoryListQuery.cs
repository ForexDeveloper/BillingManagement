using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Categories;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries;

public class GetCategoryListQuery : IRequest<List<GetCategoryListVm>>
{
    public GetCategoryListQuery()
    {
    }

    public class CategoryListQueryHandler : BaseQueryHandler, IRequestHandler<GetCategoryListQuery, List<GetCategoryListVm>>
    {
        private readonly ICategoryReadOnlyRepository _categoryReadOnlyRepository;

        public CategoryListQueryHandler(ICategoryReadOnlyRepository categoryReadOnlyRepository)
        {
            _categoryReadOnlyRepository = categoryReadOnlyRepository;
        }

        public async Task<List<GetCategoryListVm>> Handle(GetCategoryListQuery request, CancellationToken cancellationToken)
        {
            var result = await _categoryReadOnlyRepository.GetListAsync();
            var categoryVm = result.Select(x => new GetCategoryListVm
            {
                Title = x.Title,
                Id = x.Id,
                ParentId = x.ParentId,
                Items = x.Categories.Select(c => new GetCategoryListVm
                {
                    Title = c.Title,
                    Id = c.Id,
                    ParentId = c.ParentId,
                }).ToList()
            }).ToList();
            return categoryVm;
        }
    }

}
