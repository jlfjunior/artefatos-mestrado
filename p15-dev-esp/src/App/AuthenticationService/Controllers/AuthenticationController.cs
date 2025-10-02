using Application.Authentication.Authentication.Command.ChangePassword;
using Application.Authentication.Authentication.Command.DeleteUser;
using Application.Authentication.Authentication.Command.Login;
using Application.Authentication.Authentication.Command.Register;
using Application.Authentication.Authentication.Query.Users;
using Domain.Models.Authentication;
using Domain.Models.Authentication.Login;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthenticationController> _logger;
    private readonly SignInManager<ApplicationUserModel> _signInManager;

    public AuthenticationController(IMediator mediator, SignInManager<ApplicationUserModel> signInManager, ILogger<AuthenticationController> logger)
    {
        _mediator = mediator;
        _logger = logger;
        _signInManager = signInManager;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel login)
    {
        try
        {
            _logger.LogDebug($"Login attempt for {login.Username}");

            if (login == null || string.IsNullOrEmpty(login.Username) || string.IsNullOrEmpty(login.Password))
            {
                _logger.LogError("Invalid login request.");
                return BadRequest("Invalid login request.");
            }

            var loginCommand = new LoginCommand() { Username = login.Username, Password = login.Password };

            var response = await _mediator.Send(loginCommand);

            if (!response.Success)
            {
                _logger.LogWarning($"User {login.Username} failed to log in due to invalid credentials.");
                return Unauthorized("Invalid credentials.");
            }

            _logger.LogDebug($"User {login.Username} logged in successfully.");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to log in user {login.Username}: {ex.Message}");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }


    [Authorize(Roles = "Admin", AuthenticationSchemes = "Bearer")]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand register)
    {
        try
        {
            _logger.LogDebug($"Register attempt for {register.UserName}");

            var response = await _mediator.Send(register);

            if (response.Success)
            {
                _logger.LogDebug($"User {register.UserName} registered successfully.");
                return Ok(response);
            }

            _logger.LogWarning($"Failed to register user {register.UserName}: {response.Message}");
            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to register user {register.UserName}: {ex.Message}");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordCommand changePassword)
    {
        try
        {
            var response = await _mediator.Send(changePassword);

            if (response.Success)
            {
                _logger.LogDebug($"User {changePassword.Username} password reset successfully.");
                return Ok(response);
            }

            _logger.LogWarning($"Failed to reset password for user {changePassword.Username}: {response.Message}");
            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogDebug($"Failed to reset password for user {changePassword.Username}: {ex.Message}");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpPost("logout")]
    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }

    [HttpGet("/api/Authentication/GetUsersPaginated")]
    public async Task<IActionResult> GetPaginated(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var result = await _mediator.Send(new GetPaginatedUsersQuery { PageNumber = pageNumber, PageSize = pageSize });

            _logger.LogInformation("Retrieved paginated users.");

            return Ok(new
            {
                Total = result.TotalCount,
                Users = result.Items,
                result.PageSize,
                result.PageNumber
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve users.");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            _logger.LogDebug($"User deleting attempt of user id {id}");

            if (id == null)
            {
                _logger.LogError("Invalid user request.");
                return BadRequest("Invalid user request.");
            }
            var deleteCommand = new DeleteUserCommand() { Id = id };

            var response = await _mediator.Send(deleteCommand);
            _logger.LogDebug($"USer {deleteCommand.Id} created successfully.");
            return Ok(response);
        }
        catch (Exception)
        {
            _logger.LogError($"Failed to delete user {id}");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }
}
