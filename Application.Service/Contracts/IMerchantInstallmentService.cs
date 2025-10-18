using System.Threading.Tasks;
using Domain.Core.Entities.FinancialDocumentAggregate;

namespace Application.Service.Contracts;

public interface IMerchantInstallmentService
{
    Task<decimal> CreateInstallments(FinancialDocument financialDocument);
}