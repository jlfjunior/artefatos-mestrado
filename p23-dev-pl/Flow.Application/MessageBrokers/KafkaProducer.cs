using Confluent.Kafka;
using System.Text.Json;

namespace Flow.Application.MessageBrokers
{
    public class KafkaProducer : IKafkaProducer
    {
        private readonly IProducer<string, string> _producer;
        private readonly string _topic;

        public KafkaProducer(string bootstrapServers, string topic)
        {
            _topic = topic;
            var config = new ProducerConfig{ 
                BootstrapServers = bootstrapServers,
                SecurityProtocol = SecurityProtocol.Plaintext
                };
            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        public async Task ProduceAsync<T>(T message)
        {
            var json = JsonSerializer.Serialize(message);

            await _producer.ProduceAsync(_topic, new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = json
            });
        }
    }
}
