using CashFlowControl.Core.Application.Commands;
using CashFlowControl.Core.Application.DTOs;
using CashFlowControl.Core.Application.Security.Helpers;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CashFlowControl.Core.Application.Handlers
{
    public class ValidateTokenHandler : IRequestHandler<ValidateTokenCommand, Result<UserTokenValidationDto>>
    {
        private readonly IConfiguration configuration;

        public ValidateTokenHandler(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        private TokenValidationParameters GetValidationParametersForBearer()
        {
            var validIssuer = configuration["Jwt:Issuer"] ?? string.Empty;
            var issuerSigningKey = configuration["Jwt:SecretKey"] ?? string.Empty;

            return new TokenValidationParameters
            {
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(issuerSigningKey)),
                ValidIssuer = validIssuer,
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                TryAllIssuerSigningKeys = true
            };
        }

        public Task<Result<UserTokenValidationDto>> Handle(ValidateTokenCommand request, CancellationToken cancellationToken)
        {
            TokenValidationParameters validationParameters;
            if (request.Scheme == "Bearer")
            {
                var IssuerSigningKey = configuration["Jwt:SecretKey"] ?? string.Empty;

                if (string.IsNullOrWhiteSpace(IssuerSigningKey))
                {
                    return Task.FromResult(Result<UserTokenValidationDto>.Failure($"Authorization scheme '{request.Scheme}' is not supported."));
                }

                validationParameters = GetValidationParametersForBearer();
            }
            else
            {
                return Task.FromResult(Result<UserTokenValidationDto>.Failure($"Unsupported scheme: {request.Scheme}"));
            }

            var jwtHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = jwtHandler.ValidateToken(request.Token, validationParameters, out var validatedToken);

                if (principal?.Identity is ClaimsIdentity identity)
                {
                    var userIdClaim = identity.FindFirst("UserId")?.Value;
                    if (string.IsNullOrEmpty(userIdClaim))
                    {
                        return Task.FromResult(Result<UserTokenValidationDto>.Failure("The JWT token does not have the expected claims."));
                    }

                    return Task.FromResult(Result<UserTokenValidationDto>.Success(new UserTokenValidationDto(identity)));
                }

                return Task.FromResult(Result<UserTokenValidationDto>.Failure("Token validation failed."));
            }
            catch (SecurityTokenException ex)
            {
                return Task.FromResult(Result<UserTokenValidationDto>.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                return Task.FromResult(Result<UserTokenValidationDto>.Failure($"An error occurred during token validation: {ex.Message}"));
            }
        }
    }
}
