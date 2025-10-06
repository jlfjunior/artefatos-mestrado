using System.Security.Claims;

namespace Flow.Core.Security.Tokens
{
    public interface ITokenValidate
    {
        ClaimsPrincipal TokenValidator(string token);
        Guid GetUserId(string token);
    }
}
