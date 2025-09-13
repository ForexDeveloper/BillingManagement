using Application.Query.Base;
using Application.Query.ViewModels;

namespace Application.Query.QueryModels;

public class PlanSimpleListQueryModel
{
    public int Id { get; set; }
    public string Title { get; set; }
}
public class GetPlanForGridQueryModel : BasePaginatedListQueryResult<GetAllPlanQueryModel>
{

}