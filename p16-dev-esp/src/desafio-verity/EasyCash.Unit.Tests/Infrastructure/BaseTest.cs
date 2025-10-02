using Bogus;
using EasyCash.Abstractions;

namespace EasyCash.Unit.Tests.Infrastructure;

public abstract class BaseTest
{
    protected Faker Faker = new("pt_BR");

    public static T AssertDomainEventWasPublished<T>(IEntity entity)
        where T : DomainEventBase
    {
        T? domainEvent = entity.GetEvents().OfType<T>().SingleOrDefault();

        if (domainEvent == null)
        {
            throw new Exception($"{typeof(T).Name} was not published");
        }

        return domainEvent;
    }
}
