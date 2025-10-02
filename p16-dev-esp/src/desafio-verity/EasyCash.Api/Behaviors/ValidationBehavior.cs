using EasyCash.Abstractions.Exceptions;
using FluentValidation;
using MediatR;
using ValidationException = EasyCash.Abstractions.Exceptions.ValidationException;

namespace EasyCash.Api.Behaviors;

internal sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IBaseRequest
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationErrors = _validators
            .Select(async validator => await validator.ValidateAsync(context, cancellationToken))
            .Where(validationResult => validationResult.Result.Errors.Any())
            .SelectMany(validationResult => validationResult.Result.Errors)
            .Select(validationFailure => new ValidationError(
                validationFailure.PropertyName,
                validationFailure.ErrorMessage))
            .ToList();

        if (validationErrors.Any())
        {
            throw new ValidationException(validationErrors);
        }

        return await next();
    }
}