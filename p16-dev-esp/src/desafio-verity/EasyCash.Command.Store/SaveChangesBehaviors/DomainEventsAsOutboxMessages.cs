using EasyCash.Abstractions;
using EasyCash.Abstractions.Interfaces;
using EasyCash.Command.Store.Contexts;
using EasyCash.Command.Store.Repositories;
using Newtonsoft.Json;

namespace EasyCash.Command.Store.SaveChangesBehaviors;

internal sealed class DomainEventsAsOutboxMessages
{
    private static readonly JsonSerializerSettings JsonSerializerSettings = new()
    {
        TypeNameHandling = TypeNameHandling.All
    };

    internal static void AddDomainEventsAsOutboxMessages(ApplicationDbContext context, IDateTimeProvider dateTimeProvider)
    {
        var outboxMessages = context
            .ChangeTracker
            .Entries<IEntity>()
            .Select(entry => entry.Entity)
            .SelectMany(entity =>
            {
                var domainEvents = entity.GetEvents();

                entity.ClearEvents();

                return domainEvents;
            })
            .Select(domainEvent => new OutboxMessage(
                domainEvent.Id,
                dateTimeProvider.UtcNow,
                domainEvent.GetType().Name,
                JsonConvert.SerializeObject(domainEvent, JsonSerializerSettings)))
            .ToList();

        context.AddRange(outboxMessages);
    }
}
