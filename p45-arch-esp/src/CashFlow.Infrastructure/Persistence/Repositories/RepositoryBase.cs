using System.Data.Common;
using CashFlow.Shared.Extensions;
using Dapper;

namespace CashFlow.Infrastructure.Persistence.Repositories;

/// <summary>
/// Base para repositórios Dapper com helpers de comando.
/// </summary>
/// <param name="connection">Conexão scoped compartilhada no escopo da requisição.</param>
public abstract class RepositoryBase(ScopedDbConnection connection)
{
    /// <summary>
    /// Executa consulta que retorna no máximo uma linha.
    /// </summary>
    /// <typeparam name="T">Tipo da linha retornada.</typeparam>
    /// <param name="sql">Comando SQL.</param>
    /// <param name="param">Parâmetros do comando.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Linha encontrada ou default.</returns>
    protected Task<T?> QuerySingleOrDefaultAsync<T>(
        string sql,
        object? param = null,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(
            (dbConnection, ct) => dbConnection.QuerySingleOrDefaultAsync<T>(Command(sql, param, ct)),
            cancellationToken);

    /// <summary>
    /// Executa consulta que retorna múltiplas linhas.
    /// </summary>
    /// <typeparam name="T">Tipo das linhas retornadas.</typeparam>
    /// <param name="sql">Comando SQL.</param>
    /// <param name="param">Parâmetros do comando.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Lista somente leitura com os resultados.</returns>
    protected async Task<IReadOnlyList<T>> QueryAsync<T>(
        string sql,
        object? param = null,
        CancellationToken cancellationToken = default)
    {
        var rows = await ExecuteAsync(
            (dbConnection, ct) => dbConnection.QueryAsync<T>(Command(sql, param, ct)),
            cancellationToken);

        return rows.ToReadOnlyList();
    }

    /// <summary>
    /// Executa consulta escalar.
    /// </summary>
    /// <typeparam name="T">Tipo do valor escalar.</typeparam>
    /// <param name="sql">Comando SQL.</param>
    /// <param name="param">Parâmetros do comando.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Valor escalar retornado pelo banco.</returns>
    protected async Task<T> ExecuteScalarAsync<T>(
        string sql,
        object? param = null,
        CancellationToken cancellationToken = default)
    {
        var result = await ExecuteAsync(
            (dbConnection, ct) => dbConnection.ExecuteScalarAsync<T>(Command(sql, param, ct)),
            cancellationToken);

        return result!;
    }

    /// <summary>
    /// Executa comando sem retorno de conjunto de resultados.
    /// </summary>
    /// <param name="sql">Comando SQL.</param>
    /// <param name="param">Parâmetros do comando.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Task representando a operação assíncrona.</returns>
    protected Task ExecuteCommandAsync(
        string sql,
        object? param = null,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(
            (dbConnection, ct) => dbConnection.ExecuteAsync(Command(sql, param, ct)),
            cancellationToken);

    /// <summary>
    /// Executa ação com conexão resolvida no escopo atual.
    /// </summary>
    /// <typeparam name="T">Tipo do resultado.</typeparam>
    /// <param name="action">Ação a executar com a conexão.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Resultado retornado pela ação.</returns>
    protected async Task<T> ExecuteAsync<T>(
        Func<DbConnection, CancellationToken, Task<T>> action,
        CancellationToken cancellationToken = default)
    {
        var dbConnection = await connection.GetConnectionAsync(cancellationToken);
        return await action(dbConnection, cancellationToken);
    }

    /// <summary>
    /// Executa ação com conexão resolvida no escopo atual.
    /// </summary>
    /// <param name="action">Ação a executar com a conexão.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Task representando a operação assíncrona.</returns>
    protected async Task ExecuteAsync(
        Func<DbConnection, CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        var dbConnection = await connection.GetConnectionAsync(cancellationToken);
        await action(dbConnection, cancellationToken);
    }

    /// <summary>
    /// Cria definição de comando Dapper com token de cancelamento.
    /// </summary>
    /// <param name="sql">Comando SQL.</param>
    /// <param name="param">Parâmetros do comando.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Definição de comando para Dapper.</returns>
    protected static CommandDefinition Command(string sql, object? param, CancellationToken cancellationToken) =>
        new(sql, param, cancellationToken: cancellationToken);
}
