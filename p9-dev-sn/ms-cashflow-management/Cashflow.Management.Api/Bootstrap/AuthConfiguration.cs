using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Cashflow.Management.Api.Bootstrap
{
    public static class AuthConfiguration
    {
        public static void ConfigureAuth(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.Authority = configuration["Keycloack:Authority"];
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = false,
                        RoleClaimType = "roles"
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = context =>
                        {
                            var claimsIdentity = context.Principal!.Identity as ClaimsIdentity;

                            var roleClaims = context.Principal.FindFirst("realm_access")?.Value;
                            if (roleClaims != null)
                            {
                                var parsed = System.Text.Json.JsonDocument.Parse(roleClaims);
                                if (parsed.RootElement.TryGetProperty("roles", out var roles))
                                {
                                    foreach (var role in roles.EnumerateArray())
                                    {
                                        claimsIdentity?.AddClaim(new Claim("roles", role.GetString()!));
                                    }
                                }
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("ManagerOnly", policy =>
                    policy.RequireRole("manager"));

                options.AddPolicy("Employee", policy =>
                    policy.RequireRole("employee"));
            });
        }
    }
}
