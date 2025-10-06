using CashFlow.Application.Commands;
using FluentValidation;
using MediatR;
using static CashFlow.Application.ErrorCode;

namespace CashFlow.Application.CoR;

public class ValidateCommandCoR<TRequest, TResponse> : IPipelineBehavior<TRequest, CommandResponse<TResponse>>
    where TRequest : notnull
    where TResponse : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidateCommandCoR(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public Task<CommandResponse<TResponse>> Handle(TRequest request,
        RequestHandlerDelegate<CommandResponse<TResponse>> next, CancellationToken cancellationToken)
    {
        var failures = _validators
            .Select(v => v.Validate(request))
            .SelectMany(result => result.Errors)
            .Where(f => f != null)
            .ToList();

        return failures.Any()
            ? Task.FromResult(CommandResponse<TResponse>.CreateFail(CommandInvalid, string.Join(",",
                failures.Select(x => x.ErrorMessage))))
            : next();
    }
}