using RabbitMQ.Client;

namespace CashFlow.Infrastructure.Messaging;

/// <summary>
/// Declaração de exchanges Event Message, filas e dead-letter no RabbitMQ.
/// </summary>
public static class RabbitMqTopology
{
    /// <summary>
    /// Declara exchanges fanout (ex.: na API antes de publicar).
    /// </summary>
    /// <param name="channel">Canal RabbitMQ.</param>
    /// <param name="exchanges">Nomes das exchanges fanout a declarar.</param>
    public static void DeclareExchanges(IModel channel, params string[] exchanges)
    {
        foreach (var exchange in exchanges)
        {
            channel.ExchangeDeclare(exchange, ExchangeType.Fanout, durable: true);
        }
    }

    /// <summary>
    /// Declara fila com DLQ/DLX derivados do nome e vincula às exchanges informadas.
    /// </summary>
    /// <param name="channel">Canal RabbitMQ.</param>
    /// <param name="queueName">Nome da fila principal.</param>
    /// <param name="bindExchanges">Exchanges fanout para bind (routing key vazia).</param>
    public static void DeclareQueue(IModel channel, string queueName, params string[] bindExchanges)
    {
        var deadLetterExchange = DeadLetterExchangeName(queueName);
        var deadLetterQueue = DeadLetterQueueName(queueName);

        channel.ExchangeDeclare(deadLetterExchange, ExchangeType.Fanout, durable: true);
        channel.QueueDeclare(deadLetterQueue, durable: true, exclusive: false, autoDelete: false);
        channel.QueueBind(deadLetterQueue, deadLetterExchange, routingKey: string.Empty);

        var args = new Dictionary<string, object>
        {
            ["x-dead-letter-exchange"] = deadLetterExchange
        };

        channel.QueueDeclare(
            queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: args);

        DeclareExchanges(channel, bindExchanges);

        foreach (var exchange in bindExchanges)
        {
            channel.QueueBind(queueName, exchange, routingKey: string.Empty);
        }
    }

    /// <summary>
    /// Retorna o nome da exchange dead-letter derivado da fila principal.
    /// </summary>
    /// <param name="queueName">Nome da fila principal.</param>
    /// <returns>Nome da exchange DLX.</returns>
    public static string DeadLetterExchangeName(string queueName) => $"{queueName}.dlx";

    /// <summary>
    /// Retorna o nome da fila dead-letter derivado da fila principal.
    /// </summary>
    /// <param name="queueName">Nome da fila principal.</param>
    /// <returns>Nome da fila DLQ.</returns>
    public static string DeadLetterQueueName(string queueName) => $"{queueName}.dlq";
}
