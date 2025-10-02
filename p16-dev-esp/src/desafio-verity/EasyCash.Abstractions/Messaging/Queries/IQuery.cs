using MediatR;

namespace EasyCash.Abstractions.Messaging.Queries;

public interface IQueryRequest<TQueryResponse> : IRequest<TQueryResponse>
    where TQueryResponse : class, new()
{
}