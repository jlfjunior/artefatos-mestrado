using Bogus;
using EasyCash.Command.Users.Login;
using EasyCash.Command.Users.Register;
using EasyCash.Query.Users;
using EasyCash.Unit.Tests.Infrastructure;

namespace EasyCash.Unit.Tests.Application.Users;

[CollectionDefinition(nameof(UserTestsFixtureCollection))]
public class UserTestsFixtureCollection : ICollectionFixture<UserTestsFixture> { }

public class UserTestsFixture : BaseTest
{
    public Faker<RegisterUserCommand> RegisterUserCommandFaker { get; }
    public Faker<LogInUserCommand> LoginUserCommandFaker { get; }
    public Faker<UserQueryResult> UserQueryResultFaker { get; }

    public UserTestsFixture()
    {
        RegisterUserCommandFaker = new Faker<RegisterUserCommand>()
            .CustomInstantiator(f => new RegisterUserCommand(
                f.Name.FirstName(),
                f.Name.LastName(),
                f.Internet.Email(),
                f.Internet.Password()));

        LoginUserCommandFaker = new Faker<LogInUserCommand>()
            .CustomInstantiator(f => new LogInUserCommand(
                f.Internet.Email(),
                f.Internet.Password()));

        UserQueryResultFaker = new Faker<UserQueryResult>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.FirstName, f => f.Name.FirstName())
            .RuleFor(x => x.LastName, f => f.Name.LastName())
            .RuleFor(x => x.Email, f => f.Internet.Email());
    }
}