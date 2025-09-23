using System.Collections.Generic;

namespace Application.Query.ViewModels.Categories;

public class GetCategoryListVm
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public string Title { get; set; }
    public List<GetCategoryListVm> Items { get; set; }
}
