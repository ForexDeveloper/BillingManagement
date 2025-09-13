using System.Collections.Generic;

namespace Application.Query.Base;

public abstract class BasePaginatedListQueryResult<T>
{
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public List<T> Items { get; set; }
}
