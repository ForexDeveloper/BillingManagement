using Application.Query.Base;

namespace Application.Query.ViewModels.Plans
{
    public class GetPlanForGridViewModel : BasePaginatedListQueryResult<GetAllPlanViewModel>
    {

    }

    public class PlanSimpleListViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
    }
}