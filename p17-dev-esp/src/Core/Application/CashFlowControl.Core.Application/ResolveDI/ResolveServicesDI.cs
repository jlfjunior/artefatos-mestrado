using CashFlowControl.Core.Application.Interfaces.Services;
using CashFlowControl.Core.Application.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CashFlowControl.Core.Application.ResolveDI
{
    public static class ResolveServicesDI
    {
        public static void RegistryServices(WebApplicationBuilder builder)
        {
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    builder => builder.AllowAnyOrigin()
                                      .AllowAnyMethod()
                                      .AllowAnyHeader());
            });

            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            var httpClient = new HttpClient(handler);

            builder.Services.AddHttpClient("SemValidacaoSSL", client =>
            {
                client.BaseAddress = new Uri("https://CashFlowControl");
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
            });

            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.All;
            });

            builder.Services.AddHttpClient<TransactionHttpClientService>();

            builder.Services.AddScoped<IDailyConsolidationService, DailyConsolidationService>();
            builder.Services.AddScoped<ITransactionHttpClientService, TransactionHttpClientService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
        }
    }
}
