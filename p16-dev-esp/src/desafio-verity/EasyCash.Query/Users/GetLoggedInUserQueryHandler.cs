using EasyCash.Abstractions.Authentication;
using EasyCash.Abstractions.Messaging.Queries;
using EasyCash.Query.Users.Interfaces;

namespace EasyCash.Query.Users;

internal sealed class GetLoggedInUserQueryHandler : IQueryHandler<GetLoggedInUserQuery, UserQueryResult>
{
    private readonly IUserContext _userContext;
    private readonly IUserQueryStore _userQueryStore;

    public GetLoggedInUserQueryHandler(IUserContext userContext, IUserQueryStore userQueryStore)
    {
        _userContext = userContext;
        _userQueryStore = userQueryStore;
    }

    public async Task<UserQueryResult> Handle(
        GetLoggedInUserQuery request,
        CancellationToken cancellationToken)
    {
        return await _userQueryStore.GetLoggedInUser(_userContext.IdentityId, cancellationToken);
    }
}
