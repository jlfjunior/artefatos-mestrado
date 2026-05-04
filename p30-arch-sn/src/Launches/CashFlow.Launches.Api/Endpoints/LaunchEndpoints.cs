using CashFlow.Launches.Api.Extensions;
using CashFlow.Launches.Application.Commands;
using MediatR;

namespace CashFlow.Launches.Api.Endpoints;

public static class LaunchEndpoints
{
    public static IEndpointRouteBuilder MapLaunchEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health", () =>
            Results.Ok(new { status = "ok", service = "launches-api" }));

        app.MapPost("/api/launches", RegisterLaunchAsync);

        return app;
    }

    private static Task<IResult> RegisterLaunchAsync(
        RegisterLaunchCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        return sender.SendCommand(
            command,
            id => Results.Created($"/api/launches/{id}", new { Id = id }),
            cancellationToken);
    }
}