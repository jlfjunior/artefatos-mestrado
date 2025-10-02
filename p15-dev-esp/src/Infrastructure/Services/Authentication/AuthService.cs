using Domain.Models.Authentication.Login;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Domain.Models.Authentication;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using Services.Interfaces;
using Domain.DTOs;
using Azure.Core;
using System.Threading;
using Microsoft.EntityFrameworkCore;

namespace Application.Authentication.Services;

public class AuthService(UserManager<ApplicationUserModel> _userManager,
        IConfiguration _configuration,
        ILogger<AuthService> _logger) : IAuthService
{

    public async Task<LoginResponseModel> Authenticate(string username, string password)
    {
        var user = await _userManager.FindByNameAsync(username);

        if (user == null || !await _userManager.CheckPasswordAsync(user, password))
        {
            _logger.LogWarning($"Failed login attempt for {username}");
            return new LoginResponseModel { Success = false, Message = "Invalid user or password." };
        }

        var token = GenerateJwtToken(user);
        return new LoginResponseModel { Success = true, Message = $"User {user.UserName} is logged.", Token = token };
    }

    public async Task<ApiResponse> Register(string username, string fullname, string password)
    {
        var user = new ApplicationUserModel
        {
            UserName = username,
            NormalizedUserName = username.ToUpper(),
            FullName = fullname,
        };

        var result = await _userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            var roleResult = await _userManager.AddToRoleAsync(user, "user");

            if (!roleResult.Succeeded)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = string.Join(", ", roleResult.Errors.Select(e => e.Description))
                };
            }

            return new ApiResponse
            {
                Success = true,
                Message = "User successfully registered and role assigned."
            };
        }

        return new ApiResponse
        {
            Success = false,
            Message = string.Join(", ", result.Errors.Select(e => e.Description))
        };
    }


    public async Task<ApiResponse> ChangePassword(string username, string currentPassword, string newPassword, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByNameAsync(username);
        if (user == null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = "User not found."
            };
        }

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

        if (result.Succeeded)
        {
            return new ApiResponse
            {
                Success = true,
                Message = "Password changed successfully."
            };
        }

        return new ApiResponse
        {
            Success = false,
            Message = string.Join(", ", result.Errors.Select(e => e.Description))
        };
    }

    public async Task<PaginatedResult<ApplicationUserModel>> GetPaginatedUsers(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var users = await _userManager.Users
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<ApplicationUserModel>(users, await _userManager.Users.CountAsync(), pageSize, pageNumber);
    }

    public async Task<ApiResponse> DeleteUser(string id)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                return new ApiResponse
                {
                    Success = true,
                    Message = $"User {user.UserName} successfully deleted."
                };
            }

            return new ApiResponse
            {
                Success = false,
                Message = string.Join(", ", result.Errors.Select(e => e.Description))
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting user with ID {id}: {ex.Message}");
            return new ApiResponse
            {
                Success = false,
                Message = "An error occurred while deleting the user."
            };
        }
    }


    private string GenerateJwtToken(ApplicationUserModel user)
    {
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];

        var role = _userManager.GetRolesAsync(user).Result.FirstOrDefault();

        if (role == null)
        {
            _logger.LogError($"User {user.UserName} has no role assigned.");
            throw new InvalidDataException($"User {user.UserName} has no role assigned.");
        }

        var claims = new[]
        {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

        var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
