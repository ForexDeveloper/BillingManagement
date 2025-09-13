using Domain.Base;
using Domain.Core.Entities.ClosedloopAggregate;

namespace Domain.Core.Entities.PlanAggregate
{
    public class PlanClosedloop : BaseEntity<int>
    {
        public Closedloop ClosedLoop { get; set; }
        public int ClosedLoopId { get; set; }
        public Plan Plan{ get; set; }
        public int PlanId { get; set; }

        private PlanClosedloop()
        {
                
        }
        public PlanClosedloop(int closedLoopId)
        {
            ClosedLoopId = closedLoopId;
        }
        public void  SetPlanClosedloop(int closedLoopId)
        {
            ClosedLoopId = closedLoopId;
        }
    }
}
