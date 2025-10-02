using MassTransit;
using Microsoft.AspNetCore.Builder;

namespace CashFlowControl.LaunchControl.Infrastructure.ResolveDI
{
    public static class MassTransitDI
    {
        public static void Registry(WebApplicationBuilder builder)
        {
            var host = builder.Configuration["RabbitMQ:Host"] ?? "";
            var port = Convert.ToUInt16(builder.Configuration["RabbitMQ:Port"] ?? "0");
            var userName = builder.Configuration["RabbitMQ:Username"] ?? "";
            var password = builder.Configuration["RabbitMQ:Password"] ?? "";

            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password) || port <= 0)
            {
                throw new InvalidOperationException("RabbitMq settings were not provided.");
            }

            builder.Services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(host, port, "/", h =>
                    {
                        h.Username(userName);
                        h.Password(password);
                    });
                });
            });
        }
    }
}
