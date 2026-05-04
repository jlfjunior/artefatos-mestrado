using CashFlow.BuildingBlocks.Results;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace CashFlow.BuildingBlocks.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next(cancellationToken);

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToList();

        if (failures.Count == 0)
            return await next(cancellationToken);

        var error = CreateValidationError(failures);

        return CreateValidationResponse(error);
    }

    private static Error CreateValidationError(IReadOnlyCollection<ValidationFailure> failures)
    {
        var details = failures
            .GroupBy(failure => string.IsNullOrWhiteSpace(failure.PropertyName) ? "Request" : failure.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(failure => failure.ErrorMessage)
                    .Distinct()
                    .ToArray());

        return new Error(
            Code: "validation.failed",
            Message: "One or more validation errors occurred.",
            Type: ErrorType.Validation,
            Details: details);
    }

    private static TResponse CreateValidationResponse(Error error)
    {
        if (typeof(TResponse) == typeof(Result))
            return (TResponse)(object)Result.Failure(error);

        if (typeof(TResponse).IsGenericType &&
            typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var responseType = typeof(TResponse).GetGenericArguments()[0];
            var resultType = typeof(Result<>).MakeGenericType(responseType);

            var failureMethod = resultType.GetMethod(
                nameof(Result<object>.Failure),
                [typeof(Error)]);

            if (failureMethod is null)
                throw new InvalidOperationException(
                    $"Could not find Failure factory method for {resultType.Name}.");

            var failureResult = failureMethod.Invoke(null, [error]);

            return (TResponse)failureResult!;
        }

        throw new ValidationException([new ValidationFailure(string.Empty, error.Message)]);
    }
}