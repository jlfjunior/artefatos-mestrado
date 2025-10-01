using Microsoft.AspNetCore.Identity;

namespace CashFlow.IdentifyServer.Api.Endpoints;

public static class ResetPasswordEndpoint
{
    public record ResetPasswordRequest(string Email, string NewPassword);

    public static WebApplication MapResetPasswordEndpoint(this WebApplication app)
    {
        app.MapPost("/reset-password", async (ResetPasswordRequest req, UserManager<IdentityUser<Guid>> userManager) =>
        {
            var user = await userManager.FindByEmailAsync(req.Email);
            if (user is null)
                return Results.NotFound();

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await userManager.ResetPasswordAsync(user, token, req.NewPassword);

            if (!resetResult.Succeeded)
                return Results.BadRequest(resetResult.Errors);

            return Results.Ok();
        });

        return app;
    }
}
