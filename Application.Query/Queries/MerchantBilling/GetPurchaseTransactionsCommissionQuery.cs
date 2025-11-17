using MediatR;
using System.Linq;
using System.Threading;
using Domain.Core.Constants;
using System.Threading.Tasks;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.MerchantBillings;
using Domain.Core.Entities.BillingAggregate.Exceptions;

namespace Application.Query.Queries.MerchantBilling;

public sealed record GetPurchaseTransactionsCommissionQuery(int TenantId, long Id) : IRequest<GetPurchaseTransactionsCommissionVm>
{
    public long Id { get; set; } = Id;

    public int TenantId { get; set; } = TenantId;
}

public sealed class GetPurchaseTransactionsCommissionQueryHandler(IMerchantBillingReadOnlyRepository repository)
    : IRequestHandler<GetPurchaseTransactionsCommissionQuery, GetPurchaseTransactionsCommissionVm>
{
    public async Task<GetPurchaseTransactionsCommissionVm> Handle(GetPurchaseTransactionsCommissionQuery query,
        CancellationToken cancellationToken)
    {
        var commission = await repository.GetPurchaseTransactionsCommissionAsync(query);

        if (commission == null)
        {
            throw new BillingNotFoundException(BillingConstants.NotFoundMessage);
        }

        var purchaseCommission = new GetPurchaseTransactionsCommissionVm()
        {
            Id = commission.BillingId,
            Message = commission.Message,
            MerchantId = commission.MerchantId,
            FinalAmount = commission.FinalAmount,
            MainContractId = commission.MainContractId,
            CalculatedAmount = commission.CalculatedAmount,
            TransactionsCount = commission.TransactionsCount,
            TransactionsAmount = commission.TransactionsAmount,
            TieredCalculatedLevels = commission.TieredCalculatedLevels?.Select(p => new TieredCalculatedLevelVm()
            {
                Number = p.Number,
                Commission = p.Commission,
                TransactionsAmount = p.TransactionsAmount
            }),
            Contracts = commission.Contracts.Select(p => new GetMerchantBillingContractVm()
            {
                Id = p.Id,
                Status = p.Status,
                EndDate = p.EndDate,
                StartDate = p.StartDate,
                Description = p.Description,
                FixedAmountCommission = p.FixedAmountCommission,
                FixedPercentageCommission = p.FixedPercentageCommission,
                PeriodMaxCommissionAmount = p.PeriodMaxCommissionAmount,
                PeriodMinCommissionAmount = p.PeriodMinCommissionAmount,
                CommissionCalculationType = p.CommissionCalculationType,
                CommissionCalculationTypeTitle = p.CommissionCalculationTypeTitle,
                TransactionMaxCommissionAmount = p.TransactionMaxCommissionAmount,
                TransactionMinCommissionAmount = p.TransactionMinCommissionAmount,
                TieredCommissions = p.TieredCommissions?.Select(tieredCommission =>
                {
                    var selected = commission.TieredCalculatedLevels?.Any(r => r.TieredCommission == tieredCommission) ?? false;

                    if (p.Id != commission.MainContractId) selected = false;

                    return new TieredCommissionVm()
                    {
                        Selected = selected,
                        ToAmount = tieredCommission.ToAmount,
                        MaxAmount = tieredCommission.MaxAmount,
                        MinAmount = tieredCommission.MinAmount,
                        Percentage = tieredCommission.Percentage,
                        FromAmount = tieredCommission.FromAmount
                    };
                })
            })
        };

        return purchaseCommission;
    }
}