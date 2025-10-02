using MediatR;

namespace EasyCash.Abstractions.Messaging.Commands;

public interface ICommand : IRequest<ICommandResult>, IBaseCommand
{
}

public interface ICommand<out TCommandResult> : IRequest<TCommandResult>, IBaseCommand
    where TCommandResult : ICommandResult
{
}

public interface IBaseCommand
{

}