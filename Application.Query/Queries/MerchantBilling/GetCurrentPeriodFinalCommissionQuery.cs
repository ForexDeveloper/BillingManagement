using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.MerchantBillings;

namespace Application.Query.Queries.MerchantBilling;

public sealed record GetCurrentPeriodFinalCommissionQuery(long Id, int TenantId) : IRequest<GetCurrentPeriodFinalCommissionViewModel>
{
    public long Id { get; set; } = Id;

    public int TenantId { get; set; } = TenantId;
}

public sealed class GetCurrentPeriodFinalCommissionQueryHandler(IMerchantBillingReadOnlyRepository repository)
    : IRequestHandler<GetCurrentPeriodFinalCommissionQuery, GetCurrentPeriodFinalCommissionViewModel>
{
    public async Task<GetCurrentPeriodFinalCommissionViewModel> Handle(GetCurrentPeriodFinalCommissionQuery query,
        CancellationToken cancellationToken)
    {
        var commission = await repository.GetCurrentPeriodFinalCommissionAsync(query);

        var activeContract = commission.Contracts.FirstOrDefault();

        if (commission.Amount < activeContract?.PeriodMinCommissionAmount)
        {
            commission.Message = $"ریال است که از حداقل مبلغ کارمزد دوره کمتر است، در نتیجه حداقل مبلغ کارمزد {activeContract.PeriodMinCommissionAmount} درنظر گرفته می شود {commission.Amount} مجموع کارمزد شما";
        }

        return commission;
    }
}