using EasyCash.Abstractions.Messaging.Queries;

namespace EasyCash.Query.Users;

public sealed record GetLoggedInUserQuery : IQueryRequest<UserQueryResult>;

public sealed class UserQueryResult
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
}
