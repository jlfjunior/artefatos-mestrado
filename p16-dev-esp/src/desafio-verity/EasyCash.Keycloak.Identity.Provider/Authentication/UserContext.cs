using EasyCash.Abstractions.Authentication;
using EasyCash.Abstractions.Exceptions;
using Microsoft.AspNetCore.Http;

namespace EasyCash.Keycloak.Identity.Provider.Authentication;

internal sealed class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool UserIdLoggedIn =>
        ((System.Security.Claims.ClaimsIdentity)(_httpContextAccessor.HttpContext?.User).Identity).IsAuthenticated;

    public Guid UserId =>
        _httpContextAccessor
            .HttpContext?
            .User
            .GetUserId() ??
        throw new BusinessException(new Error("UserPreferences.InvalidId", "Invalid user id"));

    public string IdentityId =>
        _httpContextAccessor
            .HttpContext?
            .User
            .GetIdentityId() ??
        throw new BusinessException(new Error("UserPreferences.InvalidId", "Invalid user"));
}
