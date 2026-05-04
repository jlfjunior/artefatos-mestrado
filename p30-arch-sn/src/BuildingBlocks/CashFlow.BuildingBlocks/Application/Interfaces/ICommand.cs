using CashFlow.BuildingBlocks.Results;
using MediatR;

namespace CashFlow.BuildingBlocks.Application.Interfaces;

public interface ICommand : IRequest<Result>
{
}

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}