using EasyCash.Abstractions;
using EasyCash.Abstractions.Messaging.Events;
using EasyCash.ArchitectureTests.Infrastructure;
using FluentAssertions;
using NetArchTest.Rules;
using System.Reflection;

namespace EasyCash.ArchitectureTests.Domain;

public class DomainTests : BaseTest
{
    [Fact]
    public void DomainEvents_Should_BeSealed()
    {
        TestResult result = Types.InAssemblies(DomainAssemblies)
            .That()
            .AreNotAbstract()
            .And()
            .ImplementInterface(typeof(IDomainEvent))
            .Or()
            .Inherit(typeof(DomainEventBase))
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void DomainEvent_ShouldHave_DomainEventPostfix()
    {
        TestResult result = Types.InAssemblies(DomainAssemblies)
            .That()
            .AreNotAbstract()
            .And()
            .ImplementInterface(typeof(IDomainEvent))
            .Or()
            .Inherit(typeof(DomainEventBase))
            .Should()
            .HaveNameEndingWith("DomainEvent")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Entities_ShouldHave_PrivateParameterlessConstructor()
    {
        IEnumerable<Type> entityTypes = Types.InAssemblies(DomainAssemblies)
            .That()
            .ImplementInterface(typeof(IEntity<>))
            .Or()
            .ImplementInterface(typeof(IEntity))
            .Or()
            .Inherit(typeof(EntityBase<>))
            .GetTypes()
            .Where(t => !t.IsAbstract);

        var failingTypes = new List<Type>();
        foreach (Type entityType in entityTypes)
        {
            ConstructorInfo[] constructors = entityType.GetConstructors(BindingFlags.NonPublic |
                                                                        BindingFlags.Instance);

            if (!constructors.Any(c => c.IsPrivate && c.GetParameters().Length == 0))
            {
                failingTypes.Add(entityType);
            }
        }

        failingTypes.Should().BeEmpty();
    }

    [Fact]
    public void Entities_ShouldHave_StaticFactoryMethod()
    {
        IEnumerable<Type> entityTypes = Types.InAssemblies(DomainAssemblies)
            .That()
            .ImplementInterface(typeof(IEntity<>))
            .Or()
            .ImplementInterface(typeof(IEntity))
            .Or()
            .Inherit(typeof(EntityBase<>))
            .GetTypes()
            .Where(t => !t.IsAbstract);

        var failingTypes = new List<Type>();
        foreach (Type entityType in entityTypes)
        {
            var staticMethods = entityType.GetMethods(BindingFlags.Public | BindingFlags.Static);

            if (!staticMethods.Any(x => x is { IsStatic: true, IsPublic: true, Name: "Create" }))
                failingTypes.Add(entityType);
        }

        failingTypes.Should().BeEmpty();
    }
}
