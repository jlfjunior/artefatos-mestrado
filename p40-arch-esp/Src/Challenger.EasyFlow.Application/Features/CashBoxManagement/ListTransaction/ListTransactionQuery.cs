using Challenger.EasyFlow.Application.Common.CQRS;

namespace Challenger.EasyFlow.Application.Features.CashBoxManagement.ListTransaction;

public sealed record ListTransactionQuery(Guid CashBoxId, QueryOptions Options) : Query<QueryResult>;
