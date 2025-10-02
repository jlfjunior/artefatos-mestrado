using Cashflow.Management.Application.Shared;
using System.Diagnostics;

namespace Cashflow.Management.Api.Bootstrap
{
    public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (BusinessException ex)
            {
                var methodInfo = GetExceptionOrigin(ex);
                _logger.LogError(ex, "BusinessException: {method} - {message}", methodInfo, ex.Message);

                context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                context.Response.ContentType = "application/json";
                var response = new { Error = ex.Message };
                await context.Response.WriteAsJsonAsync(response);
            }
            catch (NotFoundException ex)
            {
                var methodInfo = GetExceptionOrigin(ex);
                _logger.LogError(ex, "NotFoundException: {method} - {message}", methodInfo, ex.Message);

                context.Response.StatusCode = StatusCodes.Status404NotFound;
                context.Response.ContentType = "application/json";
                var response = new { Error = ex.Message };
                await context.Response.WriteAsJsonAsync(response);
            }
            catch (Exception ex)
            {
                var methodInfo = GetExceptionOrigin(ex);
                _logger.LogError(ex, "InternalException: {method} - {message}", methodInfo, ex.Message);

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";
                var response = new { Error = "Sorry, an internal error has occurred..." };
                await context.Response.WriteAsJsonAsync(response);
            }
        }

        private static string GetExceptionOrigin(Exception ex)
        {
            var stackTrace = new StackTrace(ex, true);
            var frame = stackTrace.GetFrames()?.FirstOrDefault(f =>
                f.GetMethod()?.DeclaringType != null &&
                f.GetMethod()?.DeclaringType?.Namespace?.StartsWith("Cashflow") == true);

            if (frame != null)
            {
                var method = frame.GetMethod();
                return $"{method?.DeclaringType?.FullName}.{method?.Name}";
            }

            return "UnknownMethod";
        }
    }
}
