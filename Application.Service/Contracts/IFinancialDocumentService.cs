using System.Threading.Tasks;
using Domain.Core.Entities.FinancialDocumentAggregate;

namespace Application.Service.Contracts;

public interface IFinancialDocumentService
{
    Task<decimal> CalculateRefundCommission(FinancialDocument financialDocument);
}