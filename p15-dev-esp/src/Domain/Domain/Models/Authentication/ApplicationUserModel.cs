using Microsoft.AspNetCore.Identity;

namespace Domain.Models.Authentication;
public class ApplicationUserModel : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
