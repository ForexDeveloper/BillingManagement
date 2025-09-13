using System.Collections.Generic;

namespace Application.Query.QueryModels;

public class CategoryListQueryModel
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public string Title { get; set; }
    public List<CategoryListQueryModel> Categories { get; set; }
}
