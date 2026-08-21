using System.Data.Common;
using CashFlow.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Npgsql;

namespace CashFlow.Infrastructure.Persistence;

/// <summary>
/// Factory de conexões e transação ambiente para unidade de trabalho.
/// </summary>
/// <param name="options">Opções de conexão com PostgreSQL.</param>
public sealed class DbConnectionFactory(IOptions<DatabaseOptions> options)
{
    private static readonly AsyncLocal<DbTransaction?> AmbientTransaction = new();

    internal async Task<DbConnection> OpenNewConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new NpgsqlConnection(options.Value.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    internal static DbConnection? GetAmbientConnection() =>
        AmbientTransaction.Value?.Connection;

    internal static void SetAmbientTransaction(DbTransaction transaction) =>
        AmbientTransaction.Value = transaction;

    internal static void ClearAmbientTransaction() =>
        AmbientTransaction.Value = null;
}
