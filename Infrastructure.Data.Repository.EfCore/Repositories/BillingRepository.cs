using Domain.Core.Entities.BillingAggregate;
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;

namespace Infrastructure.Data.Repository.EfCore.Repositories;

public sealed class BillingRepository(ApplicationDbContext applicationDbContext)
    : Repository<Billing, long>(applicationDbContext), IBillingRepository
{
}