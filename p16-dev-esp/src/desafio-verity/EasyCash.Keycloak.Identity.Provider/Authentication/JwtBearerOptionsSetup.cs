using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace EasyCash.Keycloak.Identity.Provider.Authentication;

internal sealed class JwtBearerOptionsSetup : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly AuthenticationOptions _authenticationOptions;

    public JwtBearerOptionsSetup(IOptions<AuthenticationOptions> authenticationOptions)
    {
        _authenticationOptions = authenticationOptions.Value;
    }

    public void Configure(JwtBearerOptions options)
    {
        options.Audience = _authenticationOptions.Audience;
        options.MetadataAddress = _authenticationOptions.MetadataUrl;
        options.RequireHttpsMetadata = _authenticationOptions.RequireHttpsMetadata;
        options.TokenValidationParameters.ValidIssuer = _authenticationOptions.Issuer;

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                // If the request is for the SignalR hub...
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notifications"))
                {
                    // Read the token out of the query string and assign it to the context token
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    }

    public void Configure(string? name, JwtBearerOptions options)
    {
        Configure(options);
    }
}
