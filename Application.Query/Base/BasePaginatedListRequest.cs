namespace Application.Query.Base;

public abstract class BasePaginatedListRequest
{
    private int pageIndex = 1;
    public int PageIndex
    {
        get
        {
            return pageIndex;
        }
        set
        {
            if (value > 0)
                pageIndex = value;
            else
                pageIndex = 1;
        }
    }

    private int pageSize = 10;
    public int PageSize
    {
        get
        {
            return pageSize;
        }
        set
        {
            if (value > 0 && value <= 200)
                pageSize = value;
            else if (value > 200)
                pageSize = 200;
            else
                pageSize = 10;
        }
    }

    public string? SortColumn { get; set; }
    public SortDirection? SortDirection { get; set; } = Base.SortDirection.Descending;
    public string? SearchValue { get; set; }
}

public enum SortDirection
{
    Ascending,
    Descending
}