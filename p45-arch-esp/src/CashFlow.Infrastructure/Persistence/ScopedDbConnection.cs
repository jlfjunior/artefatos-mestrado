using System.Data.Common;

namespace CashFlow.Infrastructure.Persistence;

/// <summary>
/// Conexão compartilhada no escopo da requisição, liberada ao final do scope.
/// </summary>
/// <param name="factory">Factory de conexões PostgreSQL.</param>
public sealed class ScopedDbConnection(DbConnectionFactory factory) : IAsyncDisposable
{
    private DbConnection? _connection;

    /// <summary>
    /// Obtém uma conexão do banco de dados.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Conexão do banco de dados.</returns>
    public async Task<DbConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
    {
        var ambient = DbConnectionFactory.GetAmbientConnection();
        if (ambient is not null)
        {
            return ambient;
        }

        if (_connection is null)
        {
            _connection = await factory.OpenNewConnectionAsync(cancellationToken);
        }

        return _connection;
    }

    /// <summary>
    /// Libera a conexão do banco de dados.
    /// </summary>
    /// <returns>ValueTask concluída após o dispose da conexão.</returns>
    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }
    }
}
