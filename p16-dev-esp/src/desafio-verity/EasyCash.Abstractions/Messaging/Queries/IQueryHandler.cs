using MediatR;

namespace EasyCash.Abstractions.Messaging.Queries;

public interface IQueryHandler<in TQueryRequest, TQueryResponse> : IRequestHandler<TQueryRequest, TQueryResponse>
    where TQueryRequest : IQueryRequest<TQueryResponse>
    where TQueryResponse : class, new()
{
}