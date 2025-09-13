using Domain.Base;

namespace Domain.Core.Entities.PlanAggregate
{
    public class PlanDetailInstallment : BaseEntity<int>
    {
        public PlanDetail PlanDetail { get; set; }
        public int PlanDetailId { get; set; }
        public int NumberOfInstallment { get; set; }

        private PlanDetailInstallment()
        {
        }

        public PlanDetailInstallment(int numberOfInstallment)
        {
            NumberOfInstallment = numberOfInstallment;
        }

        public void SetNumberOfInstallment(int numberOfInstallment)
        {
            NumberOfInstallment = numberOfInstallment;
        }
    }
}
