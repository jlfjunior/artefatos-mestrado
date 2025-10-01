using CashFlow.Application.Common.PubSub;
using MassTransit;
using MassTransit.Configuration;
using System.Net.Mime;

namespace CashFlow.Infrastructure.Common.PubSub;

public class BusPublisher(IBus bus) : IBusPublisher
{
    public async Task SendAsRawJsonAsync<TEvent>(TEvent @event, CancellationToken cancellationToken)
    {
        var exchangeName = typeof(TEvent).Name;
        var endpointUri = new Uri($"exchange:{exchangeName}");

        var endpoint = await bus.GetSendEndpoint(endpointUri);

        await endpoint.Send(@event, context =>
        {
            context.ContentType = new ContentType("application/json");
            context.Serializer = RawJsonSerializerFactory.Serializer;
        }, cancellationToken);
    }
}

public static class RawJsonSerializerFactory
{
    private static readonly SystemTextJsonRawMessageSerializerFactory _factory = new();
    public static IMessageSerializer Serializer => _factory.CreateSerializer();
}
