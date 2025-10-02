using System.Security.Claims;

namespace CashFlowControl.Core.Application.DTOs
{
    public class UserTokenValidationDto(ClaimsIdentity claimsIdentity)
    {
        public ClaimsIdentity ClaimsIdentity { get; } = claimsIdentity;
    }
}
