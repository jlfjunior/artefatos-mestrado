namespace EasyCash.Query.Users.Interfaces;

public interface IUserQueryStore
{
    Task<UserQueryResult> GetLoggedInUser(string id, CancellationToken cancellationToken);

    Task<Guid> GetUserByEmailAsync(string email, CancellationToken cancellationToken);
}