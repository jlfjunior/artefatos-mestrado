using Elastic.CommonSchema;
using Foundation.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Extensions.Authentication
{
    public static class AutenticationExtension
    {
        public static OpenApiSecurityScheme OpenApiSecurityScheme => new OpenApiSecurityScheme()
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JSON Web Token based security",
        };

        public static OpenApiSecurityRequirement OpenApiSecurityRequirement => new OpenApiSecurityRequirement()
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] {}
            }
        };

        public static OpenApiContact OpenApiContact => new OpenApiContact()
        {
            Name = "Welberty Lopes",
            Email = "welberty.betinho@gmail.com",
            Url = new Uri("")
        };

        public static OpenApiLicense OpenApiLicense=> new OpenApiLicense()
        {
            Name = "Free License",
            Url = new Uri("")
        };
        public static IServiceCollection AddAuthentication(this IServiceCollection services, AppSettings appSettings)
        {
            var key = Encoding.ASCII.GetBytes("chave_secreta_aqui"); // Substitua pela sua chave secreta
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = appSettings.Jwt.Issuer,
                    ValidAudience = appSettings.Jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey
            (Encoding.UTF8.GetBytes(appSettings.Jwt.Key)),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true
                };
            });

            services.AddAuthorization();
            return services;
        }


        public static IEndpointRouteBuilder MapLogin(this IEndpointRouteBuilder endpointRouteBuilder, AppSettings appSettings)
        {
            endpointRouteBuilder.MapPost("/security/getToken", [AllowAnonymous] (UserDto user) =>
            {

                if (user.UserName == "testUser" && user.Password == "testPass")
                {
                    var issuer = appSettings.Jwt.Issuer;
                    var audience = appSettings.Jwt.Audience;
                    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.Jwt.Key));
                    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                    var jwtTokenHandler = new JwtSecurityTokenHandler();

                    var key = Encoding.ASCII.GetBytes(appSettings.Jwt.Key);
                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new[]
                        {
                new Claim("Id", "1"),
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            }),

                        Expires = DateTime.UtcNow.AddHours(6),
                        Audience = audience,
                        Issuer = issuer,
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
                    };

                    var token = jwtTokenHandler.CreateToken(tokenDescriptor);

                    var jwtToken = jwtTokenHandler.WriteToken(token);

                    return Results.Ok(jwtToken);
                }
                else
                {
                    return Results.Unauthorized();
                }
            });

            return endpointRouteBuilder;
        }
    }

    record UserDto(string UserName, string Password);

}
