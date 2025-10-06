using System.Data;
using System.Data.Common;
using Bc.CashFlow.Domain.User;
using Dapper;
using Microsoft.Extensions.Logging;

namespace Bc.CashFlow.IO.DbContext;

public class UserRepository : IUserRepository
{
	private readonly DbConnection _dbConnection;
	private readonly DbTransaction _dbTransaction;
	private readonly UserFactory _factory;

	// ReSharper disable once NotAccessedField.Local
	private readonly ILogger<UserRepository> _logger;

	public UserRepository(
		ILogger<UserRepository> logger,
		DbConnection dbConnection,
		DbTransaction dbTransaction,
		UserFactory factory)
	{
		_logger = logger;
		_dbConnection = dbConnection;
		_dbTransaction = dbTransaction;
		_factory = factory;
	}

	public async Task<IEnumerable<IUser>> GetUsers(
		string? username,
		DateTime? createdSince,
		DateTime? createdUntil,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		DynamicParameters parameters = new();

		parameters.Add("@Username", username, DbType.String);
		parameters.Add("@CreatedSince", createdSince, DbType.DateTime);
		parameters.Add("@CreatedUntil", createdUntil, DbType.DateTime);

		return (await _dbConnection.QueryAsync<UserDto>(
				"usp_SelectUsers",
				parameters,
				commandType: CommandType.StoredProcedure,
				transaction: _dbTransaction))
			.Select(
				row =>
					_factory.Create(
						row.UserId,
						row.Username,
						row.CreatedAt
					));
	}

	public async Task<IUser?> GetSingleUser(
		int id,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		DynamicParameters parameters = new();

		parameters.Add("@UserId", id, DbType.Int32);

		return (await _dbConnection.QueryAsync<UserDto>(
				"usp_SelectUser",
				parameters,
				commandType: CommandType.StoredProcedure,
				transaction: _dbTransaction))
			.Select(
				row =>
					_factory.Create(
						row.UserId,
						row.Username,
						row.CreatedAt
					))
			.SingleOrDefault();
	}
}

file record UserDto
{
	public required int UserId { get; init; }
	public required string Username { get; init; }
	public required string PasswordSalt { get; init; }
	public required string PasswordHash { get; init; }
	public required DateTime CreatedAt { get; init; }
}
