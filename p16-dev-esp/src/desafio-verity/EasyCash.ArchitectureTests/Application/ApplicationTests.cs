using EasyCash.Abstractions.Messaging.Commands;
using EasyCash.Abstractions.Messaging.Queries;
using EasyCash.ArchitectureTests.Infrastructure;
using FluentAssertions;
using FluentValidation;
using NetArchTest.Rules;
using ICommand = System.Windows.Input.ICommand;

namespace EasyCash.ArchitectureTests.Application;

public class ApplicationTests : BaseTest
{
    [Fact]
    public void CommandHandler_ShouldHave_NameEndingWith_CommandHandler()
    {
        NetArchTest.Rules.TestResult result = Types.InAssembly(ApplicationCommandAssembly)
            .That()
            .ImplementInterface(typeof(ICommandHandler<,>))
            .Should()
            .HaveNameEndingWith("CommandHandler")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Command_ShouldHave_NameEndingWith_Command()
    {
        NetArchTest.Rules.TestResult result = Types.InAssembly(ApplicationCommandAssembly)
            .That()
            .ImplementInterface(typeof(ICommand))
            .Should()
            .HaveNameEndingWith("Command")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void CommandResult_ShouldHave_NameEndingWith_CommandResult()
    {
        NetArchTest.Rules.TestResult result = Types.InAssembly(ApplicationCommandAssembly)
            .That()
            .ImplementInterface(typeof(ICommandResult))
            .Should()
            .HaveNameEndingWith("CommandResult")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void CommandHandler_Should_NotBePublic()
    {
        NetArchTest.Rules.TestResult result = Types.InAssembly(ApplicationCommandAssembly)
            .That()
            .ImplementInterface(typeof(ICommandHandler<,>))
            .Should()
            .NotBePublic()
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void QueryHandler_ShouldHave_NameEndingWith_QueryHandler()
    {
        NetArchTest.Rules.TestResult result = Types.InAssembly(ApplicationQueryAssembly)
            .That()
            .ImplementInterface(typeof(IQueryHandler<,>))
            .Should()
            .HaveNameEndingWith("QueryHandler")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Query_ShouldHave_NameEndingWith_Query()
    {
        NetArchTest.Rules.TestResult result = Types.InAssembly(ApplicationQueryAssembly)
            .That()
            .ImplementInterface(typeof(IQueryRequest<>))
            .Should()
            .HaveNameEndingWith("Query")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void QueryHandler_Should_NotBePublic()
    {
        NetArchTest.Rules.TestResult result = Types.InAssembly(ApplicationQueryAssembly)
            .That()
            .ImplementInterface(typeof(IQueryHandler<,>))
            .Should()
            .NotBePublic()
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Validator_ShouldHave_NameEndingWith_Validator()
    {
        NetArchTest.Rules.TestResult result = Types.InAssemblies(ApplicationAssemblies)
            .That()
            .Inherit(typeof(AbstractValidator<>))
            .Should()
            .HaveNameEndingWith("Validator")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
    [Fact]
    public void Validator_Should_NotBePublic()
    {
        NetArchTest.Rules.TestResult result = Types.InAssemblies(ApplicationAssemblies)
            .That()
            .Inherit(typeof(AbstractValidator<>))
            .Should()
            .NotBePublic()
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}
