using Domain.Core.Entities.CashOutRequestAggregate;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repository.EfCore.Repositories;

public class CashOutRequestRepository : ICashOutRequestRepository
{
    private readonly ApplicationDbContext _context;

    public CashOutRequestRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(CashOutRequest cashOutRequest)
    {
        await _context.CashOutRequests.AddAsync(cashOutRequest);

    }

    public Task<CashOutRequest> GetByIdAsync(long id)
    {
        return _context.CashOutRequests
            .Include(c => c.CashWallet)
            .Include(c => c.FinancialDocument).ThenInclude(c => c.FinancialDocumentPayments)
            .Include(c => c.BankAccount)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}