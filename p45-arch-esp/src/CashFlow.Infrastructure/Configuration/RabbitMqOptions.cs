namespace CashFlow.Infrastructure.Configuration;

/// <summary>
/// Configurações de conexão com RabbitMQ.
/// </summary>
public sealed class RabbitMqOptions
{
    /// <summary>Nome da seção de configuração no arquivo de configuração.</summary>
    public const string SectionName = "RabbitMq";

    /// <summary>Nome do host do RabbitMQ.</summary>
    public string HostName { get; set; } = "localhost";

    /// <summary>Porta do RabbitMQ.</summary>
    public int Port { get; set; } = 5672;

    /// <summary>Nome de usuário do RabbitMQ.</summary>
    public string UserName { get; set; } = "guest";

    /// <summary>Senha do RabbitMQ.</summary>
    public string Password { get; set; } = "guest";

    /// <summary>Virtual host do RabbitMQ.</summary>
    public string VirtualHost { get; set; } = "/";
}
