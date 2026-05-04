using CashFlow.BuildingBlocks.Application.Interfaces;
using CashFlow.BuildingBlocks.Results;
using MediatR;

namespace CashFlow.Consolidation.Api.Extensions;

public static class MediatorExtensions
{
    extension(ISender sender)
    {
        public async Task<IResult> SendCommand(ICommand command,
            CancellationToken cancellationToken = default,
            Func<IResult>? onSuccess = null)
        {
            var result = await sender.Send(command, cancellationToken);

            if (result.IsSuccess)
                return onSuccess?.Invoke() ?? Results.NoContent();

            return result.Error.ToHttpResult();
        }

        public async Task<IResult> SendCommand<T>(ICommand<T> command,
            Func<T, IResult>? onSuccess = null,
            CancellationToken cancellationToken = default)
        {
            var result = await sender.Send(command, cancellationToken);

            if (result.IsSuccess)
                return onSuccess?.Invoke(result.Value) ?? Results.Ok((object?)result.Value);

            return result.Error.ToHttpResult();
        }

        public async Task<IResult> SendQuery<T>(IQuery<Result<T>> query,
            CancellationToken cancellationToken = default,
            Func<T, IResult>? onSuccess = null)
        {
            var result = await sender.Send(query, cancellationToken);

            if (result.IsSuccess)
                return onSuccess?.Invoke(result.Value) ?? Results.Ok((object?)result.Value);

            return result.Error.ToHttpResult();
        }

        public async Task<IResult> SendQuery<T>(IQuery<T> query,
            CancellationToken cancellationToken = default,
            Func<T, IResult>? onSuccess = null)
        {
            var response = await sender.Send(query, cancellationToken);

            return onSuccess?.Invoke(response) ?? Results.Ok((object?)response);
        }
    }
}