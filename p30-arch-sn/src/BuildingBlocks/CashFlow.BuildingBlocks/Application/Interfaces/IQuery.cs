using MediatR;

namespace CashFlow.BuildingBlocks.Application.Interfaces;

public interface IQuery<out TResponse> : IRequest<TResponse>
{
}