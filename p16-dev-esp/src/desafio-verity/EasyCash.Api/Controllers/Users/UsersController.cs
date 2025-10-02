using Asp.Versioning;
using EasyCash.Abstractions.Authorization;
using EasyCash.Api.Middleware;
using EasyCash.Command.Users.Login;
using EasyCash.Command.Users.Register;
using EasyCash.Query.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Net;

namespace EasyCash.Api.Controllers.Users;

[ApiController]
[ApiVersion(ApiVersions.V1)]
[Produces("application/json")]
[Description("Users module controller")]
[ApiExplorerSettings(GroupName = "Users")]
[Route("api/v{version:apiVersion}/users")]
public class UsersController : ControllerBase
{
    private readonly ISender _sender;

    public UsersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("me")]
    [Authorize(policy: PoliciesConsts.CollaboratorUser)]
    [ProducesResponseType(typeof(UserQueryResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ExceptionHandlingMiddleware.ExceptionDetails), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(ExceptionHandlingMiddleware.ExceptionDetails), (int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> GetLoggedInUser(CancellationToken cancellationToken)
    {
        var query = new GetLoggedInUserQuery();
        return Ok(await _sender.Send(query, cancellationToken));
    }

    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterUserCommandResult), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ExceptionHandlingMiddleware.ExceptionDetails), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(ExceptionHandlingMiddleware.ExceptionDetails), (int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> Register(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        return CreatedAtAction(nameof(GetLoggedInUser), await _sender.Send(request, cancellationToken));
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(AccessTokenCommandResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ExceptionHandlingMiddleware.ExceptionDetails), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(ExceptionHandlingMiddleware.ExceptionDetails), (int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> LogIn(
        LogInUserCommand request,
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(request, cancellationToken));
    }
}
