using CashFlowControl.Core.Application.DTOs;
using CashFlowControl.Core.Application.Security.Helpers;
using CashFlowControl.Core.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace CashFlowControl.Core.Application.Interfaces.Services
{
    public interface IAuthService
    {
        string GenerateAccessToken(string userId);
        Task<RefreshToken> GenerateRefreshTokenAsync(string userId);
        Task<Result<string>> RefreshAccessTokenAsync(string refreshToken);
    }
}
