using AP.Shared.EventBus.Services;
using Application.Service.Payment;
using Domain.Core.AggregateRoots.BankAccountAggregate;
using Domain.Core.AggregateRoots.BankAggregate;
using Domain.Core.AggregateRoots.ProjectAggregate;
using Domain.Core.AggregateRoots.TransferRequestAggregate;
using Domain.Core.AggregateRoots.TransferRequestAggregate.Services.ExternalProvider.ProviderFactory;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Job
{
    public interface IProcessTransferRequestJob
    {
        Task<Unit> ExecuteAsync(CancellationToken cancellationToken);
    }

    public class ProcessTransferRequestJob : IProcessTransferRequestJob
    {
        private readonly ILogger<ProcessTransferRequestJob> _logger;
        private readonly IBankAccountRepository _bankAccountRepository;
        private readonly IBankRepository _bankRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ITransferRequestRepository _transferRequestRepository;
        private readonly IAPEventBusPublisher _eventPublisher;
        private readonly IMoneyTransferProviderFactory _moneyTransferProviderFactory;

        public ProcessTransferRequestJob(
            ILogger<ProcessTransferRequestJob> logger,
            ITransferRequestRepository transferRequestRepository,
            IBankRepository bankRepository,
            IBankAccountRepository bankAccountRepository,
            IProjectRepository projectRepository,
            IAPEventBusPublisher eventPublisher,
            IMoneyTransferProviderFactory moneyTransferProviderFactory)
        {
            _logger = logger;
            _transferRequestRepository = transferRequestRepository;
            _bankAccountRepository = bankAccountRepository;
            _projectRepository = projectRepository;
            _bankRepository = bankRepository;
            _eventPublisher = eventPublisher;
            _moneyTransferProviderFactory = moneyTransferProviderFactory;
        }

        public async Task<Unit> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            var _stopwatch = new Stopwatch();
            _stopwatch.Start();

            long transferRequestId = await _transferRequestRepository.GetEligibleIdForPaymentAsync();
            if (transferRequestId <= 0)
                return Unit.Value;

            var transferRequestService = new PaymentService(_transferRequestRepository, _projectRepository, _bankRepository, _bankAccountRepository, _moneyTransferProviderFactory, _eventPublisher);
            await transferRequestService.ProcessAsync(transferRequestId, cancellationToken);

            _stopwatch.Stop();

            return Unit.Value;
        }
    }
}