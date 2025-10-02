using CashFlowControl.Core.Application.Interfaces.Repositories;
using CashFlowControl.Core.Domain.Entities;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using CashFlowControl.Core.Application.Interfaces.Services;
using MediatR;
using CashFlowControl.Core.Application.Commands.Auth;
using CashFlowControl.Core.Application.Queries.Auth;
using CashFlowControl.Core.Application.Security.Helpers;


namespace CashFlowControl.Core.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _config;
        private readonly IUserRepository _userRepository;
        private IMediator _mediator;

        public AuthService(IConfiguration config, IUserRepository userRepository, IMediator mediator)
        {
            _config = config;
            _userRepository = userRepository;
            _mediator = mediator;
        }

        public string GenerateAccessToken(string userId)
        {
            var secretToken = _config["Jwt:SecretKey"] ?? string.Empty;
            var _issuer = _config["Jwt:Issuer"] ?? string.Empty;
            var _audience = _config["Jwt:Audience"] ?? string.Empty;


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretToken));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("UserId", userId)
        };

            var token = new JwtSecurityToken(
                _issuer,
                _audience,
                claims,
                expires: DateTime.UtcNow.AddMinutes(15), 
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<RefreshToken> GenerateRefreshTokenAsync(string userId)
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)), 
                Expiration = DateTime.UtcNow.AddDays(7), 
                UserId = userId,
                IsRevoked = false
            };
            await _mediator.Send(new AuthSaveRefreshTokenCommand(refreshToken), CancellationToken.None);
            return refreshToken;
        }

        public async Task<Result<string>> RefreshAccessTokenAsync(string refreshToken)
        {
            var returnStoredToken = await _mediator.Send(new AuthGetRefreshTokenQuery(refreshToken), CancellationToken.None);

            var storedToken = returnStoredToken.Value;
            if (storedToken == null || storedToken.IsRevoked || storedToken.Expiration < DateTime.UtcNow)
                return await Task.FromResult(Result<string>.ValidationFailure("Invalid refresh token."));

            return await Task.FromResult(Result<string>.Success(GenerateAccessToken(storedToken.UserId)));
        }
    }
}
