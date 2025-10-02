using CashFlowControl.DailyConsolidation.Infrastructure.Messaging.Consumers;
using MassTransit;
using Microsoft.AspNetCore.Builder;

namespace CashFlowControl.DailyConsolidation.Infrastructure.ResolveDI
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
                x.AddConsumer<TransactionCreatedConsumer>();
                x.AddConsumer<TransactionFailedConsumer>(); 

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(host, port, "/", h =>
                    {
                        h.Username(userName);
                        h.Password(password);
                    });

                    cfg.ReceiveEndpoint("transaction-created-queue", e =>
                    {
                        e.ConfigureConsumer<TransactionCreatedConsumer>(context);

                        e.UseMessageRetry(r => r.Incremental(5, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5)));

                        e.BindDeadLetterQueue("transaction_queue_dlq"); 
                    });

                    cfg.ReceiveEndpoint("transaction_queue_dlq", e =>
                    {
                        e.ConfigureConsumer<TransactionFailedConsumer>(context);
                    });
                });
            });
        }
    }
}
