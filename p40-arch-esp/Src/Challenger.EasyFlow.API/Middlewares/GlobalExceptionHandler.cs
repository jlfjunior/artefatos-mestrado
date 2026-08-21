using System.Net.Mime;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Challenger.EasyFlow.API.Middlewares;

public sealed class GlobalExceptionHandler(IProblemDetailsService problemDetailsService,
                                           IHostEnvironment hostEnvironment,
                                           ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService = problemDetailsService;
    private readonly IHostEnvironment _hostEnvironment = hostEnvironment;
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An unhandled exception occurred.");

        var problemDetailsContext = new ProblemDetailsContext()
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Title = GetTitleForException(exception),
                Detail = _hostEnvironment.IsDevelopment() ? exception.Message : "An unexpected error occurred. Please try again later.",
                Status = GetStatusCodeForException(exception)
            }
        };

        httpContext.Response.StatusCode = problemDetailsContext.ProblemDetails.Status.Value;
        httpContext.Response.ContentType = MediaTypeNames.Application.ProblemJson;

        return await _problemDetailsService.TryWriteAsync(problemDetailsContext);
    }

    private static string GetTitleForException(Exception exception) => exception switch
    {
        ArgumentException => "Invalid argument.",
        KeyNotFoundException => "Resource not found.",
        _ => "An unexpected error occurred."
    };

    private static int GetStatusCodeForException(Exception exception) => exception switch
    {
        ArgumentException => StatusCodes.Status400BadRequest,
        KeyNotFoundException => StatusCodes.Status404NotFound,
        _ => StatusCodes.Status500InternalServerError
    };

}