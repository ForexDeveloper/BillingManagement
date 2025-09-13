using System;
using AP.Shared.EventBus.Services;
using Application.Service.Payment;
using Domain.Core.AggregateRoots.BankAccountAggregate;
using Domain.Core.AggregateRoots.BankAggregate;
using Domain.Core.AggregateRoots.ProjectAggregate;
using Domain.Core.AggregateRoots.TransferRequestAggregate;
using Domain.Core.AggregateRoots.TransferRequestAggregate.Services.ExternalProvider;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AP.Shared.Logging.Abstraction.Models;
using AP.Shared.Logging.Abstraction.Extensions;
using Application.Service.Configs;
using Microsoft.Extensions.Options;

namespace Application.Job
{
    public interface IAutoRetryTransferRequestJob
    {
        Task<Unit> ExecuteAsync(CancellationToken cancellationToken);
    }

    public class AutoRetryTransferRequestJob : IAutoRetryTransferRequestJob
    {
        private readonly IRetryService _retryService;
        private readonly IOptions<ApplicationConfig> _optionsConfig;

        public AutoRetryTransferRequestJob(
            IOptions<ApplicationConfig> optionsConfig,
            IRetryService retryService
           )
        {
            _optionsConfig = optionsConfig;
            _retryService = retryService;
        }

        public async Task<Unit> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            var dateTimeNow = DateTime.Now;
            var to = new DateTime(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, 0, 0, 0);
            var from = to.AddHours(-24);
            await _retryService.BatchRetryAsync(from, to, _optionsConfig.Value.TransferRequestMaxRetryCount, cancellationToken);
            return Unit.Value;
        }

    }
}