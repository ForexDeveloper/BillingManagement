using Application.Query.Base;
using Application.Query.QueryModels;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Plans;
using Application.Service.Helper;
using Domain.Core.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries;

public class GetPlanInstallmentsQuery : IRequest<List<PlanInstallmentsVm>>
{
    public int PlanId { get; set; }
    public int NumberOfInstallments { get; set; }
    public decimal Amount { get; set; }
    public OperationalFeeType OperationalFeeType { get; set; }

    public GetPlanInstallmentsQuery(int planId, int numberOfInstallments, decimal amount, OperationalFeeType operationalFeeType)
    {
        PlanId = planId;
        NumberOfInstallments = numberOfInstallments;
        Amount = amount;
        OperationalFeeType = operationalFeeType;
    }
}

public class GetInstallmentsQueryHandler(IPlanReadOnlyRepository planReadOnlyRepository)
    : BaseQueryHandler, IRequestHandler<GetPlanInstallmentsQuery, List<PlanInstallmentsVm>>
{
    public async Task<List<PlanInstallmentsVm>> Handle(GetPlanInstallmentsQuery query, CancellationToken cancellationToken)
    {
        var plan = await planReadOnlyRepository.GetAllInstallmentsAsync(query, cancellationToken);

        var installments = CreateInstallments(query, plan);

        return installments;
    }

    private static List<PlanInstallmentsVm> CreateInstallments(GetPlanInstallmentsQuery query, GetPlanInstallmentsQueryModel plan)
    {
        List<PlanInstallmentsVm> installments = [];

        var interestPercent = plan.InstallmentInterestPercent ?? 0;

        var now = DateTime.Now;
        PersianCalendar pc = new();
        int persianDay = pc.GetDayOfMonth(now);

        var installmentDates = DateHelper.CalculateInstallments(now, plan.BillingPeriod.GetValueOrDefault(), query.NumberOfInstallments,
            plan.InstallmentBreakType, plan.InstallmentBreak);
        var creditAmountsDto = CreditDetailsCalculator.CalculateCreditAmountIncludingOperationalFee(query.OperationalFeeType, plan.OperationFee, query.Amount);
        var creditDetail = CreditDetailsCalculator.Calculate(creditAmountsDto.CreditAmount, query.NumberOfInstallments, interestPercent);

        for (var i = 0; i < query.NumberOfInstallments; i++)
        {
            var currentInstallmentAmount = i < query.NumberOfInstallments - 1 ? creditDetail.InstallmentAmount : creditDetail.LastInstallmentAmount;
            var currentInterestAmount = i < query.NumberOfInstallments - 1 ? creditDetail.InterestAmount : creditDetail.LastInterestAmount;

            installments.Add(new PlanInstallmentsVm()
            {
                Number = i + 1,
                Amount = currentInstallmentAmount + currentInterestAmount,
                DueDate = installmentDates[i]
            });
        }

        return installments;
    }
}