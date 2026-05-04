using CashFlow.BuildingBlocks.Application.Interfaces;
using CashFlow.BuildingBlocks.Results;
using MediatR;

namespace CashFlow.Launches.Api.Extensions;

public static class MediatorExtensions
{
    public static async Task<IResult> SendCommand(
        this ISender sender,
        ICommand command,
        CancellationToken cancellationToken = default,
        Func<IResult>? onSuccess = null)
    {
        var result = await sender.Send(command, cancellationToken);

        if (result.IsSuccess)
            return onSuccess?.Invoke() ?? Results.NoContent();

        return result.Error.ToHttpResult();
    }

    public static async Task<IResult> SendCommand<T>(
        this ISender sender,
        ICommand<T> command,
        Func<T, IResult>? onSuccess = null,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(command, cancellationToken);

        if (result.IsSuccess)
            return onSuccess?.Invoke(result.Value) ?? Results.Ok(result.Value);

        return result.Error.ToHttpResult();
    }

    public static async Task<IResult> SendQuery<T>(
        this ISender sender,
        IQuery<Result<T>> query,
        CancellationToken cancellationToken = default,
        Func<T, IResult>? onSuccess = null)
    {
        var result = await sender.Send(query, cancellationToken);

        if (result.IsSuccess)
            return onSuccess?.Invoke(result.Value) ?? Results.Ok(result.Value);

        return result.Error.ToHttpResult();
    }

    public static async Task<IResult> SendQuery<T>(
        this ISender sender,
        IQuery<T> query,
        CancellationToken cancellationToken = default,
        Func<T, IResult>? onSuccess = null)
    {
        var response = await sender.Send(query, cancellationToken);

        return onSuccess?.Invoke(response) ?? Results.Ok(response);
    }
}