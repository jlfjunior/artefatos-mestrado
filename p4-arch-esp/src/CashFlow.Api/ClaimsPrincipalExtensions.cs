using System.Security.Claims;

namespace CashFlow.Api
{
    public static class ClaimsPrincipalExtensions
    {
        public static string? GetUserId(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public static Guid? GetUserIdAsNullableGuid(this ClaimsPrincipal user)
        {
            var id = user.GetUserId();

            if (Guid.TryParse(id, out var guid))
                return guid;

            return null;
        }

        public static Guid GetUserIdAsValidatedGuid(this ClaimsPrincipal user)
        {
            var id = user.GetUserId()!;

            return Guid.Parse(id);
        }
    }
}
