using Core.Bus;
using FluxoDeCaixa.Relatorios.API.BackGroundServices;
using FluxoDeCaixa.Relatorios.API.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FluxoDeCaixa.Relatorios.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApiConfiguration(Configuration);

            services.AddSingleton<IMessageBus>(
                new MessageBus(Configuration.GetSection("MessageQueueConnection:MessageBus").Value)
            ).AddHostedService<AccountEntryIntegrationHandler>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseApiConfiguration(env);
        }
    }
}
