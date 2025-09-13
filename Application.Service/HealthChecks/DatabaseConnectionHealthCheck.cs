using Domain.Core.UnitOfWorkContracts;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Service.HealthChecks;

public class DatabaseConnectionHealthCheck : IHealthCheck
{
    private readonly IApplicationDbContextUnitOfWork _applicationDbContextUnitOfWork;

    public DatabaseConnectionHealthCheck(IApplicationDbContextUnitOfWork applicationDbContextUnitOfWork)
    {
        _applicationDbContextUnitOfWork = applicationDbContextUnitOfWork;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
    {
        try
        {
            var canConnectDbContext = await _applicationDbContextUnitOfWork.CanConnectDatabaseAsync(cancellationToken);

            if (canConnectDbContext)
            {
                return HealthCheckResult.Healthy("Database is reachable.");
            }
            else
            {
                return HealthCheckResult.Unhealthy($"Database is not reachable.");
            }
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("An error occurred while checking the database.", ex);
        }
    }
}