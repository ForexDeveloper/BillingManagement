using Shared.Logging.Abstraction.Extensions;
using Shared.Logging.Abstraction.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.MediatR.Configurations;
using System.Diagnostics;

namespace Shared.MediatR.Behaviors
{
    public class LogBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<TRequest> _logger;
        private readonly MediatRConfiguration _configs;

        public LogBehavior(ILogger<TRequest> logger, IOptions<MediatRConfiguration> configs)
        {
            _logger = logger;
            if (configs is null)
                throw new ArgumentNullException(nameof(configs));
            _configs = configs.Value;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (next is null)
                throw new ArgumentNullException(nameof(next));

            var serviceName = request.ToString() ?? "";
            TResponse? result = default;

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            try
            {
                return result = await next();
            }
            catch (System.Exception exception)
            {
                stopWatch.Stop();

                _logger.LogCritical(new LogStruct()
                {
                    Message = exception.Message,
                    ServiceName = serviceName,
                    InputParams = request,
                    ResponseTimeStopWatcher = stopWatch,
                    Results = result ?? default!,
                    Exception = exception,
                    Tags = LogMessageTag.Internal
                }, _configs.FullLog);

                throw;
            }
            finally
            {
                stopWatch.Stop();

                _logger.LogTrace(new LogStruct()
                {
                    ServiceName = serviceName,
                    InputParams = request,
                    ResponseTimeStopWatcher = stopWatch,
                    Results = result ?? default!,
                    Tags = LogMessageTag.Internal
                });
            }
        }

      
    }
}
