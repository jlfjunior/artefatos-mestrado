using CashFlowControl.Core.Application.Commands;
using CashFlowControl.Core.Application.DTOs;
using CashFlowControl.Core.Application.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CashFlowControl.Permissions.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private IMediator _mediator;



        public AuthController(IAuthService authService, IMediator mediator)
        {
            _authService = authService;
            _mediator = mediator;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModelDTO model)
        {
            var ResultRegisterUser = await _mediator.Send(new AuthRegisterUserCommand(model), CancellationToken.None);
            if (!ResultRegisterUser.IsSuccess)
            {
                if (ResultRegisterUser.HasValidationErrors)
                {
                    return BadRequest(new { Errors = ResultRegisterUser.ValidationErrors });
                }

                return StatusCode(500, ResultRegisterUser.SystemError);
            }

            var (result, user) = ResultRegisterUser.Value;

            if (result == null || !result.Succeeded)
                return BadRequest(result?.Errors);

            var accessToken = _authService.GenerateAccessToken(user.Id);
            var refreshToken = await _authService.GenerateRefreshTokenAsync(user.Id);

            return Ok(new
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModelDTO model)
        {
            var ResultAuthenticate = await _mediator.Send(new AuthAuthenticateCommand(model), CancellationToken.None);
            if (!ResultAuthenticate.IsSuccess)
            {
                if (ResultAuthenticate.HasValidationErrors)
                {
                    return BadRequest(new { Errors = ResultAuthenticate.ValidationErrors });
                }

                return StatusCode(500, ResultAuthenticate.SystemError);
            }

            var (result, user) = ResultAuthenticate.Value;

            if (result == null || !result.Succeeded || user == null)
                return Unauthorized();

            var accessToken = _authService.GenerateAccessToken(user.Id);
            var refreshToken = await _authService.GenerateRefreshTokenAsync(user.Id);

            return Ok(new
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token
            });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshTokenAsync([FromBody] RefreshTokenRequestDTO request)
        {
            try
            {
                var returnRefreshToken = await _authService.RefreshAccessTokenAsync(request.RefreshToken);
                if (!returnRefreshToken.IsSuccess)
                {
                    if (returnRefreshToken.HasValidationErrors)
                    {
                        return BadRequest(new { Errors = returnRefreshToken.ValidationErrors });
                    }

                    return StatusCode(500, returnRefreshToken.SystemError);
                }
                var newAccessToken = returnRefreshToken.Value;

                return Ok(new { AccessToken = newAccessToken });
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }
    }
}
