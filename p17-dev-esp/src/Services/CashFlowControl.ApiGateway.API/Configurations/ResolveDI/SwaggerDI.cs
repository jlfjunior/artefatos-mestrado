using Microsoft.OpenApi.Models;

namespace CashFlowControl.ApiGateway.API.Configurations.ResolveDI
{
    public static class SwaggerDI
    {
        public static void Registry(WebApplicationBuilder builder)
        {
            var docLaunchcontrol = builder.Configuration["DocsSwagger:Launchcontrol"] ?? "";
            var docDailyconsolidation = builder.Configuration["DocsSwagger:Dailyconsolidation"] ?? "";
            var docPermissions = builder.Configuration["DocsSwagger:Permissions"] ?? "";


            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "CashFlowControl API Gateway",
                    Version = "v1",
                    Description = "Este é o gateway de API do CashFlowControl. Ele atua como um ponto único de entrada para todas as APIs internas do sistema. O gateway gerencia as requisições, realiza roteamento, e garante a autenticação e autorização dos usuários por meio de tokens JWT."
                });

                options.DocInclusionPredicate((docName, apiDesc) =>
                {
                    var relativePath = apiDesc.RelativePath ?? "";
                    return !relativePath.StartsWith("ocelot");
                });

                options.AddServer(new OpenApiServer { Url = docLaunchcontrol });
                options.AddServer(new OpenApiServer { Url = docDailyconsolidation });
                options.AddServer(new OpenApiServer { Url = docPermissions });

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
