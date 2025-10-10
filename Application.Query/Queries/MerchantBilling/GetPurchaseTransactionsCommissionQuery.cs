using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.MerchantBillings;

namespace Application.Query.Queries.MerchantBilling;

public sealed record GetPurchaseTransactionsCommissionQuery(int TenantId, long Id) : IRequest<GetPurchaseTransactionsCommissionViewModel>
{
    public long Id { get; set; } = Id;

    public int TenantId { get; set; } = TenantId;
}

public sealed class GetCPurchaseTransactionsCommissionQueryHandler(IMerchantBillingReadOnlyRepository repository)
    : IRequestHandler<GetPurchaseTransactionsCommissionQuery, GetPurchaseTransactionsCommissionViewModel>
{
    public async Task<GetPurchaseTransactionsCommissionViewModel> Handle(GetPurchaseTransactionsCommissionQuery query,
        CancellationToken cancellationToken)
    {
        var commission = await repository.GetPurchaseTransactionsCommissionAsync(query);

        var activeContract = commission.Contracts.FirstOrDefault();

        if (commission.Amount < activeContract?.PeriodMinCommissionAmount)
        {
            commission.Message = $"ریال است که از حداقل مبلغ کارمزد دوره کمتر است، در نتیجه حداقل مبلغ کارمزد {activeContract.PeriodMinCommissionAmount} درنظر گرفته می شود {commission.Amount} مجموع کارمزد شما";
        }

        return commission;
    }
}