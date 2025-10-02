using Microsoft.OpenApi.Models;

namespace CashFlowControl.Permissions.API.Configurations.ResolveDI
{
    public static class SwaggerDI
    {
        public static void Registry(WebApplicationBuilder builder)
        {
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Permissions API",
                    Version = "v1",
                    Description = "API responsável pelas permissões."
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Insira o token JWT no campo abaixo (não inclua 'Bearer ' no início). Exemplo: 12345abcdef",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                        new string[] { }
                    }
                });
            });
        }
    }
}
