using MediatR;

namespace EasyCash.Abstractions.Messaging.Commands;

public interface ICommandHandler<in TCommandRequest> : IRequestHandler<TCommandRequest, ICommandResult>
    where TCommandRequest : ICommand<ICommandResult>, IRequest<ICommandResult>
{
}

public interface ICommandHandler<in TCommandRequest, TCommandResult> : IRequestHandler<TCommandRequest, TCommandResult>
    where TCommandRequest : ICommand<TCommandResult>
    where TCommandResult : ICommandResult
{
}