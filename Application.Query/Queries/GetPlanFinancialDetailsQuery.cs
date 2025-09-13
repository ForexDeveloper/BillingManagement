using Application.Query.Base;
using Application.Query.ViewModels.Plans;
using Application.Service.Helper;
using Domain.Core.Entities.PlanAggregate;
using Domain.Core.Enums;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries;

public class PlanFinancialDetailsDto
{
    public OperationalFeeType OperationalFeeType { get; set; }
    public decimal CreditAmount { get; set; }
    public int PlanDetailInstallmentId { get; set; }
}

public class GetPlanFinancialDetailsQuery : IRequest<List<PlanFinancialDetailsVm>>
{
    public GetPlanFinancialDetailsQuery(List<PlanFinancialDetailsDto> planFinancialDetailsDtos)
    {
        PlanFinancialDetailsDtos = planFinancialDetailsDtos;
    }

    public List<PlanFinancialDetailsDto> PlanFinancialDetailsDtos { get; set; }
}

public class GetPlanFinancialDetailsQueryHandler : BaseQueryHandler, IRequestHandler<GetPlanFinancialDetailsQuery, List<PlanFinancialDetailsVm>>
{
    private readonly IPlanRepository _planRepository;

    public GetPlanFinancialDetailsQueryHandler(IPlanRepository planRepository)
    {
        _planRepository = planRepository;
    }

    public async Task<List<PlanFinancialDetailsVm>> Handle(GetPlanFinancialDetailsQuery request, CancellationToken cancellationToken)
    {
        List<PlanFinancialDetailsVm> result = [];

        var planDetailInstallments = await _planRepository.GetPlanDetailInstallmentsAsync(request.PlanFinancialDetailsDtos.Select(x => x.PlanDetailInstallmentId).ToList());

        foreach (var item in request.PlanFinancialDetailsDtos)
        {
            var planDetailInstallment = planDetailInstallments.FirstOrDefault(x => x.Id == item.PlanDetailInstallmentId);
            var creditAmountsDto = CreditDetailsCalculator.CalculateCreditAmountIncludingOperationalFee(item.OperationalFeeType, planDetailInstallment.PlanDetail.OperationFee, item.CreditAmount);
            var creditDetail = CreditDetailsCalculator.Calculate(creditAmountsDto.CreditAmount, planDetailInstallment.NumberOfInstallment, planDetailInstallment.PlanDetail.InterestPercent);

            result.Add(new PlanFinancialDetailsVm
            {
                PlanId = planDetailInstallment.PlanDetail.PlanId,
                PlanName = planDetailInstallment.PlanDetail.Plan.Title,
                InterestPercent = planDetailInstallment.PlanDetail.InterestPercent == null ? 0 : planDetailInstallment.PlanDetail.InterestPercent.Value,
                OperationalFeeType = item.OperationalFeeType,
                TotalRepayableAmount = creditDetail.RepayableCreditAmount,
                InstallmentAmount = creditDetail.InstallmentAmount,
                InterestAmount = creditDetail.InterestAmount,
                NumberOfInstallments = creditDetail.NumberOfInstallments,
                CreditAmount = creditAmountsDto.CreditAmount,
                InitialCreditAmount = item.CreditAmount,
                OperationalFeeAmount = creditAmountsDto.OperationalFeeAmount,
                IsMaxTotalCreditExceeded = planDetailInstallment.PlanDetail.Plan.ExceedsMaxTotalCredit(item.CreditAmount)
            });
        }

        return result;
    }
}