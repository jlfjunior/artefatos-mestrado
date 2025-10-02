using CashFlowControl.Core.Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace CashFlowControl.Core.Application.Security
{
    public class BearerAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory loggerFactory,
        UrlEncoder encoder,
        IMediator mediator
    ) : AuthenticationHandler<AuthenticationSchemeOptions>(options, loggerFactory, encoder)
    {
        private readonly ILogger<BearerAuthenticationHandler> _logger = loggerFactory.CreateLogger<BearerAuthenticationHandler>();

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
                return AuthenticateResult.NoResult();

            if (authorizationHeader.Count == 0)
                return AuthenticateResult.NoResult();

            var authHeader = AuthenticationHeaderValue.Parse(authorizationHeader!);
            if (!authHeader.Scheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase))
                return AuthenticateResult.Fail("Missing authorization schema");

            var scheme = "Bearer";
            if ((authHeader.Parameter?.Length ?? 0) == 0)
                return AuthenticateResult.Fail("Missing authorization token");

            var routeData = Context.GetRouteData();
            var controller = routeData.Values["controller"]?.ToString();
            var action = routeData.Values["action"]?.ToString();

            var validationResult = await mediator.Send(new ValidateTokenCommand(authHeader.Parameter!, scheme), CancellationToken.None);

            if (validationResult.IsSuccess)
            {
                var principal = new ClaimsPrincipal(validationResult.Value!.ClaimsIdentity);
                return AuthenticateResult.Success(new AuthenticationTicket(principal, scheme));
            }
            else
            {
                Response.StatusCode = (Int32)HttpStatusCode.Unauthorized;
                return AuthenticateResult.Fail(nameof(HttpStatusCode.Unauthorized));
            }

        }
    }
}
