using Microsoft.AspNetCore.Identity;

namespace CashFlow.IdentifyServer.Api.Endpoints
{
    public static class RegisterEndpoint
    {
        public record RegisterRequest(string Email, string Password);

        public static WebApplication MapRegisterEndpoint(this WebApplication app)
        {
            app.MapPost("/register", async (RegisterRequest req, UserManager<IdentityUser<Guid>> userManager) =>
            {
                var user = new IdentityUser<Guid> { UserName = req.Email, Email = req.Email };
                var result = await userManager.CreateAsync(user, req.Password);

                if (!result.Succeeded)
                    return Results.BadRequest(result.Errors);

                return Results.Ok(new { user.Id, user.Email });
            });

            return app;
        }
    }
}
