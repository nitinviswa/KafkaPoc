using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class KafkaConsumerService : BackgroundService
{
    private readonly IConsumer<string, string> _consumerA;
    private readonly IProducer<string, string> _producerA;
    private readonly ILogger<KafkaConsumerService> _logger;

    public KafkaConsumerService(ILogger<KafkaConsumerService> logger)
    {
        _logger = logger;

        var consumerConfigA = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            GroupId = "service-b-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
        };

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = "localhost:9092"
        };

        _consumerA = new ConsumerBuilder<string, string>(consumerConfigA).Build();
        _producerA = new ProducerBuilder<string, string>(producerConfig).Build();

        _consumerA.Subscribe("service-a-topic");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Started consuming messages from service-a-topic...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = await Task.Run(() => _consumerA.Consume(stoppingToken), stoppingToken);
                var message = consumeResult.Message.Value;

                if (message.Contains("ServiceA"))
                {
                    _logger.LogInformation($"Message consumed from service-a-topic: {message}");

                    bool isProduced = await TryProduceAsync("service-b-topic", $"Response - {message}", 3);

                    if (isProduced)
                    {
                        _logger.LogInformation($"Message replicated to service-b-topic: {message}");
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to produce message to service-b-topic after 3 attempts: {message}");
                    }
                }
            }
            catch (ConsumeException ex)
            {
                _logger.LogError($"Error consuming message: {ex.Error.Reason}");
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError($"Error producing message: {ex.Error.Reason}");
            }

            await Task.Delay(500, stoppingToken);
        }
    }

    private async Task<bool> TryProduceAsync(string topic, string value, int maxRetries)
    {
        int attempt = 0;
        while (attempt < maxRetries)
        {
            attempt++;
            try
            {
                await _producerA.ProduceAsync(topic, new Message<string, string> { Value = value });
                return true; 
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError($"Attempt {attempt} failed to produce message: {ex.Error.Reason}");
            }
        }
        return false; 
    }

    public override void Dispose()
    {
        _consumerA.Close();
        _consumerA.Dispose();
        _producerA.Dispose();
        base.Dispose();
    }
}

