using Entries.Application.DTOs;
using Entries.Domain.Interfaces;
using Entries.Domain.Entities;
using Entries.Domain.Events;
using Entries.Domain.Repositories;
using Entries.Domain.ValueObjects;

namespace Entries.Application.UseCases.CreateEntry
{
    public sealed class CreateEntryHandler(
        IEntryRepository repository,
        IMessagePublisher publisher)
    {
        public async Task<EntryResponse> HandleAsync(
            CreateEntryRequest request,
            CancellationToken cancellationToken = default)
        {
            var money = Money.Create(request.Amount, request.Currency);

            var entry = Entry.Create(
                amount: money,
                type: request.Type,
                description: request.Description,
                date: request.Date);

            await repository.AddAsync(entry, cancellationToken);

            foreach (var domainEvent in entry.DomainEvents.OfType<EntryCreatedEvent>())
            {
                await publisher.PublishAsync(domainEvent, "entry.created", cancellationToken);
            }

            entry.ClearDomainEvents();

            return new EntryResponse(
                entry.Id,
                entry.Amount.Amount,
                entry.Amount.Currency,
                entry.Type,
                entry.Description,
                entry.Date,
                entry.CreatedAt);
        }
    }
}
