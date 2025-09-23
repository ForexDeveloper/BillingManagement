using Application.Query.ReadOnlyRepositoryContracts;
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;

namespace Infrastructure.Data.Repository.EfCore.ReadonlyRepositories;

public class FinancialDocumentReadOnlyRepository : IFinancialDocumentReadOnlyRepository
{
    private readonly ReadonlyApplicationDbContext _context;

    public FinancialDocumentReadOnlyRepository(ReadonlyApplicationDbContext readonlyApplicationDbContext)
    {
        _context = readonlyApplicationDbContext;
    }

}