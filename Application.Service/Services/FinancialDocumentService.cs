using System.Threading.Tasks;
using Application.Service.Helper;
using Application.Service.Contracts;
using Domain.Core.Entities.FinancialDocumentAggregate;

namespace Application.Service.Services;

public sealed class FinancialDocumentService(IFinancialDocumentRepository repository) : IFinancialDocumentService
{
    public async Task<decimal> CalculateRefundCommission(FinancialDocument financialDocument)
    {
        decimal commission;

        var parentId = financialDocument.ParentId!.Value;

        var purchaseCommission = await repository.GetPurchaseCommission(parentId);

        var purchaseTransaction = await repository.GetPurchaseTransaction(parentId);

        var sumOfRefundTransactions = await repository.GetSumOfRefundTransactions(parentId);

        if (purchaseTransaction == sumOfRefundTransactions + financialDocument.Amount)
        {
            var sumOfRefundCommissions = await repository.GetSumOfRefundCommissions(parentId);

            commission = purchaseCommission - sumOfRefundCommissions;
        }
        else
        {
            commission = RoundHelper.RoundAmount(purchaseCommission * financialDocument.Amount / purchaseTransaction);
        }

        return commission;
    }
}