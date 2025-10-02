using MediatR;
using Serilog.Context;

namespace EasyCash.Api.Behaviors;

internal sealed class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IBaseRequest
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        string requestName = request.GetType().Name;

        try
        {
            _logger.LogInformation("Executing request {RequestName}", requestName);

            try
            {
                TResponse result = await next();

                _logger.LogInformation("Request {RequestName} processed successfully", requestName);

                return result;
            }
            catch (Exception ex)
            {
                using (LogContext.PushProperty("Error", ex.Message, true))
                {
                    _logger.LogError("Request {RequestName} processed with error", requestName);
                }

                throw;
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Request {RequestName} processing failed", requestName);

            throw;
        }
    }
}
