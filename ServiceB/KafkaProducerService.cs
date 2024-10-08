using Confluent.Kafka;

public class KafkaProducerService : IDisposable
{
    private readonly IProducer<string, string> _producer;

    public KafkaProducerService(string bootstrapServers)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers
        };
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task<bool> PublishMessageAsync(string topic, MessageDto messageDto, int maxRetries = 3)
    {
        var payload = $"{{ \"Message\": \"{messageDto.Message}\", \"Origin\": \"{messageDto.Origin}\", \"Date & Time\" : \"{messageDto.Dtime}\" }}";

        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                await _producer.ProduceAsync(topic, new Message<string, string> { Value = payload });
                Console.WriteLine($"Produced message: {payload} to topic: {topic}");
                return true; 
            }
            catch (ProduceException<string, string> ex)
            {
                Console.WriteLine($"Attempt {attempt + 1} failed to produce message: {ex.Error.Reason}");
            }
        }
        return false;
    }

    public void Dispose()
    {
        _producer.Dispose();
    }
}


