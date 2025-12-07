using System.Threading.Tasks;
using Domain.Core.Entities.FinancialDocumentAggregate;
using Domain.Core.Entities.TenantMerchantContractAggregate;

namespace Application.Service.Contracts;

public interface IMerchantInstallmentService
{
    Task<decimal> CreatePurchaseInstallments(TenantMerchantContract contract, FinancialDocument financialDocument);

    Task<decimal> CreateRefundInstallments(TenantMerchantContract contract, FinancialDocument financialDocument);
}