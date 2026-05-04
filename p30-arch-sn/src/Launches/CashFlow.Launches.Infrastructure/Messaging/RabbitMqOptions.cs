namespace CashFlow.Launches.Infrastructure.Messaging;

public sealed class RabbitMqOptions
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "cashflow";
    public string Password { get; set; } = "cashflow";
    public string VirtualHost { get; set; } = "/";
    public string ExchangeName { get; set; } = "cashflow.launches";
    public string QueueName { get; set; } = "cashflow.consolidation.launch-registered";
    public string RoutingKey { get; set; } = "launch.registered";
}