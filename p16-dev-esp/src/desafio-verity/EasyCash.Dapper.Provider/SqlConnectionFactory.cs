using EasyCash.Abstractions.Interfaces;
using Npgsql;
using System.Data;

namespace EasyCash.Dapper.Provider;

internal sealed class SqlConnectionFactory : ISqlConnectionFactory, IDisposable
{
    private readonly string _connectionString;
    private IDbConnection _connection;

    public SqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection CreateConnection()
    {
        _connection = new NpgsqlConnection(_connectionString);
        _connection.Open();

        return _connection;
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }
}
