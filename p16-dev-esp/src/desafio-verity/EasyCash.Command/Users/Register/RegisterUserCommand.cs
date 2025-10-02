using EasyCash.Abstractions.Messaging.Commands;

namespace EasyCash.Command.Users.Register;

public sealed record RegisterUserCommand(
    string Email,
    string FirstName,
    string LastName,
    string Password) : ICommand<RegisterUserCommandResult>;

public sealed record RegisterUserCommandResult(Guid Id) : ICommandResult;