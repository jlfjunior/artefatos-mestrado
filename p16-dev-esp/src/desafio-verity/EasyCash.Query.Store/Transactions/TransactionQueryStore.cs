using Dapper;
using EasyCash.Abstractions.Interfaces;
using EasyCash.Abstractions.Messaging.Queries.PagedQueries;
using EasyCash.Query.Store.Transactions.Dto;
using EasyCash.Query.Transactions.Get;
using EasyCash.Query.Transactions.Interfaces;
using System.Data;

namespace EasyCash.Query.Store.Transactions;

internal sealed class TransactionQueryStore : ITransactionQueryStore
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public TransactionQueryStore(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<GetDailyTransactionsQueryResult> GetDailyTransactionsAsync(GetDailyTransactionsQuery query, CancellationToken cancellationToken)
    {
        var param = new DynamicParameters();
        param.Add("@date", query.Date, direction: ParameterDirection.Input);
        param.Add("@Skip", query.GetPageOffset(), direction: ParameterDirection.Input);
        param.Add("@Take", query.PageSize, direction: ParameterDirection.Input);

        var count = await _sqlConnectionFactory.CreateConnection()
            .QueryFirstOrDefaultAsync<int>(
                new CommandDefinition(TransactionQueryStoreConsts.SQL_COUNT_BY_DATE, param, cancellationToken: cancellationToken));

        if (count == 0)
            return GetDailyTransactionsQueryResult.Empty;

        var result = await _sqlConnectionFactory.CreateConnection()
            .QueryAsync<TransactionDto>(
                new CommandDefinition(TransactionQueryStoreConsts.SQL_GET_BY_DATE, param, cancellationToken: cancellationToken));

        var transactionDtos = result as TransactionDto[] ?? result.ToArray();

        return GetDailyTransactionsQueryResult.Create(transactionDtos.Select(TransactionDto.MapToResult).ToArray(), query, count);
    }
}