using Domain.Base;
using Domain.Core.Enums;
using System.Collections.Generic;

namespace Domain.Core.Entities.PlanAggregate
{
    public class PlanDetail : BaseEntity<int>
    {
        public PlanDetail(decimal? operationFee, OperationalFeeType? operationalFeeType,
            decimal? penaltyPercent, decimal? penaltyMaxAmount, decimal? penaltyMinAmount,
            decimal? interestPercent, decimal? interestMaxAmount, decimal? interestMinAmount,
            decimal? waiverPercent, decimal? waiverMaxAmount, decimal? waiverMinAmount,
            decimal? prepaymentPercent, decimal? prepaymentMinAmount, decimal? prepaymentMaxAmount)
        {
            OperationFee = operationFee;
            OperationalFeeType = operationalFeeType;
            PenaltyPercent = penaltyPercent;
            PenaltyMaxAmount = penaltyMaxAmount;
            PenaltyMinAmount = penaltyMinAmount;
            InterestPercent = interestPercent;
            InterestMaxAmount = interestMaxAmount;
            InterestMinAmount = interestMinAmount;
            WaiverPercent = waiverPercent;
            WaiverMaxAmount = waiverMaxAmount;
            WaiverMinAmount = waiverMinAmount;
            PrepaymentPercent = prepaymentPercent;
            PrepaymentMaxAmount = prepaymentMaxAmount;
            PrepaymentMinAmount = prepaymentMinAmount;
        }

        public void SetPlanDetail(decimal? operationFee, OperationalFeeType? operationalFeeType,
            decimal? penaltyPercent, decimal? penaltyMaxAmount, decimal? penaltyMinAmount,
            decimal? interestPercent, decimal? interestMaxAmount, decimal? interestMinAmount,
            decimal? waiverPercent, decimal? waiverMaxAmount, decimal? waiverMinAmount,
            decimal? prepaymentPercent, decimal? prepaymentMinAmount, decimal? prepaymentMaxAmount)
        {
            OperationFee = operationFee;
            OperationalFeeType = operationalFeeType;
            PenaltyPercent = penaltyPercent;
            PenaltyMaxAmount = penaltyMaxAmount;
            PenaltyMinAmount = penaltyMinAmount;
            InterestPercent = interestPercent;
            InterestMaxAmount = interestMaxAmount;
            InterestMinAmount = interestMinAmount;
            WaiverPercent = waiverPercent;
            WaiverMaxAmount = waiverMaxAmount;
            WaiverMinAmount = waiverMinAmount;
            PrepaymentPercent = prepaymentPercent;
            PrepaymentMaxAmount = prepaymentMaxAmount;
            PrepaymentMinAmount = prepaymentMinAmount;
        }

        public int PlanId { get; private set; }
        public Plan Plan { get; private set; }
        public List<PlanDetailInstallment> PlanDetailInstallments { get; private set; }

        public void SetPlanDetailInstallment(PlanDetailInstallment planDetailInstallment)
        {
            PlanDetailInstallments ??= new List<PlanDetailInstallment>();
            PlanDetailInstallments.Add(planDetailInstallment);
        }
        public void SetPlanDetailInstallment(List<PlanDetailInstallment> planDetailInstallments)
        {
            PlanDetailInstallments = planDetailInstallments;
        }

        public decimal? OperationFee { get; private set; }
        public OperationalFeeType? OperationalFeeType { get; private set; }
        public decimal? PenaltyPercent { get; private set; }
        public decimal? PenaltyMaxAmount { get; private set; }
        public decimal? PenaltyMinAmount { get; private set; }
        public decimal? InterestPercent { get; private set; }
        public decimal? InterestMaxAmount { get; private set; }
        public decimal? InterestMinAmount { get; private set; }
        public decimal? WaiverPercent { get; private set; }
        public decimal? WaiverMaxAmount { get; private set; }
        public decimal? WaiverMinAmount { get; private set; }
        public decimal? PrepaymentPercent { get; private set; }
        public decimal? PrepaymentMinAmount { get; private set; }
        public decimal? PrepaymentMaxAmount { get; private set; }



    }
}
