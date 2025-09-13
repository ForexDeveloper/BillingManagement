using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.Logging.Abstraction.Models;
using Shared.Logging.Abstraction.Extensions;
using Shared.Middlewares.Extensions;
using System.Buffers;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO.Pipelines;
using System.Text;

namespace Shared.Middlewares;

public class LoggingMiddlewareConfig
{
    public Collection<string> ExcludedLogPaths { get; set; } = new();

    public int RequestBodyBufferSize_Byte { get; set; } = 8192;
    public int MaxRequestBodyLenghtToLog { get; set; } = 500;

    public int ResponseBodyBufferSize_Byte { get; set; } = 8192;
    public int MaxResponseBodyLenghtToLog { get; set; } = 500;
}

internal class LoggingMiddleware
{
    private readonly LoggingMiddlewareConfig _config;
    private readonly ILogger<LoggingMiddleware> _logger;
    private readonly RequestDelegate _next;

    public LoggingMiddleware(RequestDelegate next, LoggingMiddlewareConfig configOptions, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _config = configOptions;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";
        if (path == "/") path = "//";

        if (_config.ExcludedLogPaths.Any(excludedLogPath => path.Contains(excludedLogPath, StringComparison.InvariantCulture)))
        {
            await _next(context);
            return;
        }

        var stopWatch = new Stopwatch();
        stopWatch.Start();

        object inputParams = string.Empty;
        string responseBody = string.Empty;

        try
        {

            var cancellationToken = context.RequestAborted;

            //capture the request
            inputParams = await GetInputParamsAsync(context.Request, _config, cancellationToken);


            if (context.Response.ContentLength > _config.MaxResponseBodyLenghtToLog)
            {
                await _next(context);
                return;
            }


            // Create a memory stream to capture the response body
            var originalBodyStream = context.Response.Body;
            using var newBodyStream = new MemoryStream();
            context.Response.Body = newBodyStream;


            await _next(context);

            // Reset the position of the new body stream
            newBodyStream.Seek(0, SeekOrigin.Begin);

            // Read and log the response body
            responseBody = await GetResponseBodyAsync(newBodyStream, _config, cancellationToken);


            // Write the response body back to the original stream
            newBodyStream.Seek(0, SeekOrigin.Begin);
            await newBodyStream.CopyToAsync(originalBodyStream);

        }
        finally
        {
            stopWatch.Stop();

            _logger.LogTrace(new LogStruct
            {
                Message = context.Response.StatusCode.ToString(CultureInfo.InvariantCulture),
                ServiceName = context.GetServiceName(),
                InputParams = inputParams,
                Results = responseBody,
                ResponseTimeStopWatcher = stopWatch,
                Tags = LogMessageTag.Input,
                Method = context.Request.Method
            });


            object? exception = default;
            context.Items.TryGetValue("exception", out exception);

            if (context.Response.IsTechnicalError())
            {
                _logger.LogCritical(new LogStruct
                {
                    Message = context.Response.StatusCode.ToString(CultureInfo.InvariantCulture),
                    ServiceName = context.GetServiceName(),
                    InputParams = inputParams,
                    Results = responseBody,
                    Exception = exception as System.Exception,
                    ResponseTimeStopWatcher = stopWatch,
                    Tags = LogMessageTag.Input,
                    Method = context.Request.Method,

                });
            }
            if (context.Response.IsBusinessError())
            {
                _logger.LogError(new LogStruct
                {
                    Message = context.Response.StatusCode.ToString(CultureInfo.InvariantCulture),
                    ServiceName = context.GetServiceName(),
                    InputParams = inputParams,
                    Results = responseBody,
                    Exception = exception as System.Exception,
                    ResponseTimeStopWatcher = stopWatch,
                    Tags = LogMessageTag.Input,
                    Method = context.Request.Method,
                });
            }
        }

    }

    private static async ValueTask<object> GetInputParamsAsync(HttpRequest httpRequest, LoggingMiddlewareConfig config, CancellationToken cancellationToken)
    {
        try
        {
            if (httpRequest is null)
                throw new ArgumentNullException(nameof(httpRequest));

            string? queryString = httpRequest.QueryString.Value;

            var routeValues = httpRequest.RouteValues
                .Where(w => w.Key != "controller" && w.Key != "action")
                .ToArray();

            string body = await ReadReuestBodyAsync(httpRequest, config, cancellationToken);


            return new
            {
                routeValues,
                queryString,
                body
            };
        }
        catch
        {
            return default!;
        }
    }

    private static bool IsValidForLogBody(HttpRequest httpRequest, LoggingMiddlewareConfig config, out string message)
    {
        if (httpRequest.Method != "POST" && httpRequest.Method != "PUT" && httpRequest.Method != "PATCH")
        {
            message = "";
            return false;
        }

        if (httpRequest.ContentLength > config.MaxRequestBodyLenghtToLog)
        {
            message = $"body is big to log";
            return false;
        }

        message = string.Empty;
        return true;
    }

    private static async ValueTask<string> ReadReuestBodyAsync(HttpRequest httpRequest, LoggingMiddlewareConfig config, CancellationToken cancellationToken)
    {
        if (!IsValidForLogBody(httpRequest, config, out string message))
            return message;

        httpRequest.EnableBuffering();

        var pipeReader = PipeReader.Create(httpRequest.Body);
        var bodyBuilder = new StringBuilder();
        var buffer = ArrayPool<byte>.Shared.Rent(config.RequestBodyBufferSize_Byte);

        try
        {
            while (true)
            {
                ReadResult result = await pipeReader.ReadAsync(cancellationToken);
                ReadOnlySequence<byte> sequence = result.Buffer;

                foreach (var segment in sequence)
                {
                    int bytesToCopy = segment.Length;
                    if (bytesToCopy > buffer.Length)
                    {
                        ArrayPool<byte>.Shared.Return(buffer);
                        buffer = ArrayPool<byte>.Shared.Rent(bytesToCopy);
                    }

                    segment.CopyTo(buffer);
                    bodyBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytesToCopy));
                }

                // Tell the PipeReader how much of the buffer has been consumed
                pipeReader.AdvanceTo(sequence.End);

                if (result.IsCompleted)
                {
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            bodyBuilder.Append("Request was canceled by the client");
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }

        string bodyAsText = bodyBuilder.ToString();

        // Replace the request body stream with a new stream
        var newBody = new MemoryStream(Encoding.UTF8.GetBytes(bodyAsText));
        httpRequest.Body = newBody;

        // Reset the stream position to 0
        httpRequest.Body.Position = 0;

        return bodyAsText;
    }

    private static async Task<string> GetResponseBodyAsync(Stream responseBodyStream, LoggingMiddlewareConfig config, CancellationToken cancellationToken)
    {
        var pipeReader = PipeReader.Create(responseBodyStream);
        var bodyBuilder = new StringBuilder();
        var buffer = ArrayPool<byte>.Shared.Rent(config.ResponseBodyBufferSize_Byte);

        try
        {
            while (true)
            {
                ReadResult result = await pipeReader.ReadAsync(cancellationToken);
                ReadOnlySequence<byte> sequence = result.Buffer;

                foreach (var segment in sequence)
                {
                    int bytesToCopy = segment.Length;
                    if (bytesToCopy > buffer.Length)
                    {
                        ArrayPool<byte>.Shared.Return(buffer);
                        buffer = ArrayPool<byte>.Shared.Rent(bytesToCopy);
                    }

                    segment.CopyTo(buffer);
                    bodyBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytesToCopy));
                }

                pipeReader.AdvanceTo(sequence.End);

                if (result.IsCompleted)
                {
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            bodyBuilder.Append("response reading was canceled");
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }

        return bodyBuilder.ToString();
    }

}
