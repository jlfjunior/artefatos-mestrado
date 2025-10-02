using Microsoft.AspNetCore.Identity;

namespace CashFlowControl.Core.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}
