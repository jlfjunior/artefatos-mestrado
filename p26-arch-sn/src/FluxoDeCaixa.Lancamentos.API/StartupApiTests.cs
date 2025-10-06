using FluxoDeCaixa.Caixa.Configuration;
using Core.Bus;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FluxoDeCaixa.Caixa
{
    public class StartupApiTests
    {
        public StartupApiTests(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApiConfiguration(Configuration);

            services.AddSingleton<IMessageBus>(
                new MessageBus(Configuration.GetSection("MessageQueueConnection:MessageBus").Value)
            );
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseApiConfiguration(env);
        }
    }
}
