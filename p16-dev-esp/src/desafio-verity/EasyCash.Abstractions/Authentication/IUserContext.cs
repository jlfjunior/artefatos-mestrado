using EasyCash.Abstractions.Exceptions;

namespace EasyCash.Abstractions.Authentication;

public interface IUserContext
{
    bool UserIdLoggedIn { get; }

    Guid UserId { get; }

    string IdentityId { get; }

    Guid GetIdentityId()
    {
        return !Guid.TryParse(IdentityId, out var id)
            ? throw new BusinessException(new Error("UserPreferences.InvalidId", "Invalid user id"))
            : id;
    }
}
