using Dapper;
using EasyCash.Abstractions.Interfaces;
using EasyCash.Query.Users;
using EasyCash.Query.Users.Interfaces;
using System.Data;

namespace EasyCash.Query.Store.Users;

internal sealed class UserQueryStore : IUserQueryStore
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public UserQueryStore(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<UserQueryResult> GetLoggedInUser(string id, CancellationToken cancellationToken)
    {
        using IDbConnection connection = _sqlConnectionFactory.CreateConnection();

        const string sql = UserQueryStoreConsts.SQL_GET_LOGGED_IN_USER;

        var param = new DynamicParameters();
        param.Add("@IdentityId", id);
        UserQueryResult user =
            await connection.QuerySingleAsync<UserQueryResult>(new CommandDefinition(sql, param,
                cancellationToken: cancellationToken));

        return user;
    }

    public async Task<Guid> GetUserByEmailAsync(string email, CancellationToken cancellationToken)
    {
        using IDbConnection connection = _sqlConnectionFactory.CreateConnection();

        var sql = UserQueryStoreConsts.SQL_GET_USER_BY_EMAIL;

        var param = new DynamicParameters();
        param.Add("@Email", email);
        var result = await connection.QuerySingleAsync<Guid>(sql, param);

        return result;
    }
}

internal static class UserQueryStoreConsts
{
    internal const string SQL_GET_LOGGED_IN_USER = $"""
                                                    SELECT
                                                        id AS {nameof(UserQueryResult.Id)},
                                                        first_name AS {nameof(UserQueryResult.FirstName)},
                                                        last_name AS {nameof(UserQueryResult.LastName)},
                                                        email AS {nameof(UserQueryResult.Email)}
                                                    FROM users
                                                    WHERE identity_id = @IdentityId
                                                    """;

    internal const string SQL_GET_USER_BY_EMAIL = @"SELECT id
                    FROM users
                    WHERE email = @Email";
}