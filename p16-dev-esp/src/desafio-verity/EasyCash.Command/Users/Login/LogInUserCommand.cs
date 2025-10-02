using EasyCash.Abstractions.Messaging.Commands;

namespace EasyCash.Command.Users.Login;

public sealed record LogInUserCommand(string Email, string Password) : ICommand<AccessTokenCommandResult>;

public sealed record AccessTokenCommandResult(string AccessToken) : ICommandResult;
