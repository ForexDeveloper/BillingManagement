using Application.Service.Contracts;
using Domain.Core.Entities.WalletContractAggregate;
using Domain.Core.UnitOfWorkContracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.EventBus.Configurations;
using Shared.Logging.Abstraction.Extensions;
using Shared.Logging.Abstraction.Models;
using Shared.Logging.Serilog.Extensions;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Service.Worker.Workers;

public class CustomerWalletServiceWorker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly EventBusConfiguration _config;

    public CustomerWalletServiceWorker(IOptions<EventBusConfiguration> config, IServiceProvider services)
    {
        _services = services;
        _config = config.Value;
    }

    private readonly Stopwatch _stopwatch = new Stopwatch();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var fetchInterval = _config.DefaultEventFetchInterval;
        while (!stoppingToken.IsCancellationRequested)
        {
            _stopwatch.Start();

            using IServiceScope scope = _services.CreateScope();

            var walletContractRepository = scope.ServiceProvider.GetRequiredService<IWalletContractRepository>();
            var walletService = scope.ServiceProvider.GetRequiredService<ILoanWalletService>();

            var unitOfWork = scope.ServiceProvider.GetRequiredService<IApplicationDbContextUnitOfWork>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<CustomerWalletServiceWorker>>();

            logger.AddTraceId(Guid.NewGuid().ToString());

            try
            {
                var walletContractIds = await walletContractRepository.GetContractsWithCustomerWithoutWalletAsync();
                if (walletContractIds is not null && walletContractIds.Count > 0)
                {
                    foreach (var walletContractId in walletContractIds)
                    {
                        var walletContract = await walletContractRepository.GetAsync(walletContractId);

                        //if (!walletContract.IsInProcess) //
                        //{
                        //walletContract.JobStarted();
                        //await unitOfWork.SaveChangesAsync();

                        //await walletService.CreateCustomersWalletByJob(walletContract);

                        //walletContract.JobEnd();
                        //await unitOfWork.SaveChangesAsync();
                        //}
                    }
                    fetchInterval = _config.DefaultEventFetchInterval;
                }
                else
                {
                    if (fetchInterval <= 50)
                    {
                        fetchInterval = fetchInterval + 2;
                    }
                }
            }
            catch (Exception exception)
            {
                //handle IsInProcess 
                logger.LogCritical(new LogStruct()
                {
                    ServiceName = $"CustomerWalletServiceWorker{nameof(ExecuteAsync)}",
                    InputParams = "",
                    ResponseTimeStopWatcher = _stopwatch,
                    Results = "",
                    Exception = exception,
                    Message = exception.Message
                });
            }
            finally
            {
                _stopwatch.Reset();
                await Task.Delay(fetchInterval * 1000, stoppingToken);
            }

        }
    }
}
