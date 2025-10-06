using Flow.Core.Repositories;
using Flow.Core.Security.Tokens;
using Flow.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;
using System.Net;

namespace Flow.Services.Transactions.Filters
{
    public class AuthenticatedUserFilter : IAsyncAuthorizationFilter
    {
        private readonly ITokenValidate _tokenValidate;
        private readonly IUserReadOnlyRepository _userReadOnlyRepository;

        public AuthenticatedUserFilter(ITokenValidate tokenValidate, IUserReadOnlyRepository userReadOnlyRepository)
        {
            _tokenValidate = tokenValidate;
            _userReadOnlyRepository = userReadOnlyRepository;
        }
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            try
            {
                var token = TokenOnRequest(context);

                var userId = _tokenValidate.GetUserId(token);

                var userExists = await _userReadOnlyRepository.IsUserExistsById(userId);

                if (!userExists)
                {
                    throw new UnauthorizedAccessException("User Not Found");
                }
            }
            catch (SecurityTokenExpiredException)
            {
                context.Result = new UnauthorizedObjectResult(new ErrorResponse("Token expired")
                {
                    TokenIsExpired = true,
                });
            }
            catch (FlowException ex)
            {
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Result = new ObjectResult(new ErrorResponse(ex.Message));
            }
            catch
            {
                context.Result = new UnauthorizedObjectResult(new ErrorResponse("User Without Permission"));
            }
        }

        private string TokenOnRequest(AuthorizationFilterContext context)
        {
            var authorization = context.HttpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrWhiteSpace(authorization))
            {
                throw new FlowException("Unauthorized");
            }

            return authorization["Bearer ".Length..].Trim();
        }
    }
}
