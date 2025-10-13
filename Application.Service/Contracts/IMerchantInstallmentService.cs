using System.Threading.Tasks;
using Domain.Core.Entities.FinancialDocumentAggregate;
using Domain.Core.Entities.TenantMerchantContractAggregate;

namespace Application.Service.Contracts;

public interface IMerchantInstallmentService
{
    Task<decimal> CreateInstallments(TenantMerchantContract contract, FinancialDocument financialDocument);
}