using Domain.Core.Entities.InstallmentAggregate;
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;

namespace Infrastructure.Data.Repository.EfCore.Repositories;

public sealed class InstallmentRepository(ApplicationDbContext applicationDbContext)
    : Repository<Installment, long>(applicationDbContext), IInstallmentRepository;