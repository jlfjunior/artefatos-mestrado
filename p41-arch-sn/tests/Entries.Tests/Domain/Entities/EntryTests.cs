using Entries.Domain.Entities;
using Entries.Domain.Enums;
using Entries.Domain.Events;
using Entries.Domain.Exceptions;
using Entries.Domain.ValueObjects;
using FluentAssertions;

namespace Entries.Tests.Domain.Entities
{
    public sealed class EntryTests
    {
        [Fact]
        public void Create_ValidData_ShouldCreateEntry()
        {
            var money = Money.Create(100, "BRL");
            var entry = Entry.Create(money, EntryType.Credit, "Test entry", DateTime.Today);

            entry.Should().NotBeNull();
            entry.Amount.Should().Be(money);
            entry.Type.Should().Be(EntryType.Credit);
            entry.Description.Should().Be("Test entry");
            entry.Date.Should().Be(DateTime.Today);
        }

        [Fact]
        public void Create_EmptyDescription_ShouldThrowEmptyDescriptionException()
        {
            var money = Money.Create(100, "BRL");

            var act = () => Entry.Create(money, EntryType.Credit, "", DateTime.Today);

            act.Should().Throw<EmptyDescriptionException>();
        }

        [Fact]
        public void Create_WhitespaceDescription_ShouldThrowEmptyDescriptionException()
        {
            var money = Money.Create(100, "BRL");

            var act = () => Entry.Create(money, EntryType.Credit, "   ", DateTime.Today);

            act.Should().Throw<EmptyDescriptionException>();
        }

        [Fact]
        public void Create_ValidData_ShouldRaiseEntryCreatedEvent()
        {
            var money = Money.Create(100, "BRL");
            var entry = Entry.Create(money, EntryType.Credit, "Test entry", DateTime.Today);

            entry.DomainEvents.Should().ContainSingle();
            entry.DomainEvents.First().Should().BeOfType<EntryCreatedEvent>();
        }

        [Fact]
        public void Create_ValidData_EntryCreatedEvent_ShouldHaveCorrectData()
        {
            var money = Money.Create(100, "BRL");
            var entry = Entry.Create(money, EntryType.Credit, "Test entry", DateTime.Today);

            var domainEvent = entry.DomainEvents.OfType<EntryCreatedEvent>().First();

            domainEvent.EntryId.Should().Be(entry.Id);
            domainEvent.Amount.Should().Be(100);
            domainEvent.Currency.Should().Be("BRL");
            domainEvent.Type.Should().Be(EntryType.Credit);
            domainEvent.Description.Should().Be("Test entry");
        }

        [Fact]
        public void ClearDomainEvents_ShouldRemoveAllEvents()
        {
            var money = Money.Create(100, "BRL");
            var entry = Entry.Create(money, EntryType.Credit, "Test entry", DateTime.Today);

            entry.ClearDomainEvents();

            entry.DomainEvents.Should().BeEmpty();
        }

        [Fact]
        public void Create_DateShouldBeNormalizedToDateOnly()
        {
            var money = Money.Create(100, "BRL");
            var dateWithTime = new DateTime(2024, 1, 15, 10, 30, 0);

            var entry = Entry.Create(money, EntryType.Debit, "Test entry", dateWithTime);

            entry.Date.Should().Be(new DateTime(2024, 1, 15));
        }
    }
}
