using Flow.Application.MessageBrokers;
using Flow.Core.Entities;
using Moq;

namespace CommomTestsUtilities.Repositories
{
    public class KafkaProducerBuilder
    {
        private readonly Mock<IKafkaProducer> _kafkaProducer;

        public KafkaProducerBuilder() => _kafkaProducer = new Mock<IKafkaProducer>();

        public void ProduceAsync() => _kafkaProducer
            .Setup(prod => prod.ProduceAsync(It.IsAny<Transaction>()))
            .Returns(Task.CompletedTask);

        public Mock<IKafkaProducer> Builder() => _kafkaProducer;
    }
}
