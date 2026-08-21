using Challenger.EasyFlow.API.Extensions;

namespace Challenger.EasyFlow.API.Features;

public static class EndpointMapper
{
    extension(WebApplication app)
    {
        public void MapEndpoints()
        {
            var v1 = app.MapGroup("/api/v1/")
                .WithTags("Version 1");

            var cashboxes = v1.MapGroup("/cashboxes")
                .WithTags("CashBox Management");

            cashboxes.MapEndpoint<CashBoxManagement.CreateTransaction.Endpoint>();
            cashboxes.MapEndpoint<CashBoxManagement.ListTransactions.Endpoint>();
        }
    }
}