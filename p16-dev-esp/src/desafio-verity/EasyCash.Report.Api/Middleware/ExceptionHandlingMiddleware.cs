using EasyCash.Abstractions.Exceptions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EasyCash.Report.Api.Middleware;

internal sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

            ExceptionDetails exceptionDetails = GetExceptionDetails(exception);

            var problemDetails = new ProblemDetails
            {
                Status = exceptionDetails.Status,
                Type = exceptionDetails.Type,
                Title = exceptionDetails.Title,
                Detail = exceptionDetails.Detail,
            };

            if (exceptionDetails.Errors is not null)
            {
                problemDetails.Extensions["errors"] = exceptionDetails.Errors;
            }

            problemDetails.Extensions.TryAdd("requestId", context.TraceIdentifier);

            Activity? activity = context.Features.Get<IHttpActivityFeature>()?.Activity;
            problemDetails.Extensions.TryAdd("traceId", activity?.TraceId.ToHexString());

            context.Response.StatusCode = exceptionDetails.Status;

            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }

    private static ExceptionDetails GetExceptionDetails(Exception exception)
    {
        return exception switch
        {
            ValidationException validationException => new ExceptionDetails(
                StatusCodes.Status400BadRequest,
                "ValidationFailure",
                "Validation error",
                "One or more validation errors occurred",
                validationException.Errors),

            BusinessException businessException => new ExceptionDetails(
                StatusCodes.Status400BadRequest,
                "BusinessFailure",
                "Business error",
                businessException.Message,
                new[] { new { businessException.Code, businessException.Message } }),

            ConflictException conflictException => new ExceptionDetails(
                StatusCodes.Status409Conflict,
                "Conflict",
                "Conflict",
                conflictException.Message,
                null),

            _ => new ExceptionDetails(
                StatusCodes.Status500InternalServerError,
                "ServerError",
                "Server error",
                "An unexpected error has occurred",
                null)
        };
    }

    internal sealed record ExceptionDetails(
        int Status,
        string Type,
        string Title,
        string Detail,
        IEnumerable<object>? Errors);
}
