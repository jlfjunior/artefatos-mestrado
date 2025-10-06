
using Confluent.Kafka;
using Flow.Core.Entities;
using Flow.Core.Enums;
using Flow.Exceptions;
using Flow.Infra.Data;
using MongoDB.Driver;
using System.Text.Json;

namespace Flow.Services.Consolidated.Consumers
{
    public class KafkaBackgroundConsumer : BackgroundService
    {
        private readonly ILogger<KafkaBackgroundConsumer> _logger;
        private readonly IConfiguration _configuration;
        private readonly MongoDbContext _mongoDbContext;
        private readonly string _topic;
        private readonly ConsumerConfig _consumerConfig;
        private IConsumer<string, string> _consumer; 

        public KafkaBackgroundConsumer(
            ILogger<KafkaBackgroundConsumer> logger,
            IConfiguration configuration,
            MongoDbContext mongoDbContext)
        {
            _logger = logger;
            _configuration = configuration;
            _mongoDbContext = mongoDbContext;
            var kafkaSection = _configuration.GetSection("Kafka");
            _topic = kafkaSection["Topic"];

            _consumerConfig = new ConsumerConfig
            {
                BootstrapServers = kafkaSection["BootstrapServers"],
                GroupId = kafkaSection["GroupId"],
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            _consumer = new ConsumerBuilder<string, string>(_consumerConfig).Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe(_topic);
            
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Yield();
            
                        try
                        {
                            var consumeResult = _consumer.Consume(stoppingToken);
                            if (consumeResult != null)
                            {
                                var message = consumeResult.Message.Value;
                                var transaction = JsonSerializer.Deserialize<Transaction>(message);
                                if (transaction is not null)
                                {
                                    ProcessTransaction(transaction);
                                }
                            }
                        }
                        catch (ConsumeException ex)
                        {
                            _logger.LogError($"An error ocurred: {ex.Error.Reason}");
                        }
                
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Consumer failed");
            }
            finally
            {
                _consumer.Close();
                _consumer.Dispose();
            }
        }

        private void ProcessTransaction(Transaction transaction)
        {
            var collection = _mongoDbContext.DailyBalance;
            var date = transaction.Date.Date;
            var filter = Builders<DailyBalance>.Filter.Eq(x => x.Date, date);
            var dailyBalance = collection.Find(filter).FirstOrDefault();

            decimal value = transaction.Type switch
            {
                TypeTransaction.Credit => transaction.Value,
                TypeTransaction.Debit => -transaction.Value,
                _ => throw new FlowException("Invalid transaction type")
            };

            if (dailyBalance is null)
            {
                dailyBalance = new DailyBalance()
                {
                    Date = date,
                    TotalBalance = value
                };

                collection.InsertOne(dailyBalance);
            }
            else
            {
                dailyBalance.TotalBalance += value;
                collection.ReplaceOne(filter, dailyBalance);
            }
        }
    }
}
