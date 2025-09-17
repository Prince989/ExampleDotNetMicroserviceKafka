using System.Text.Json;
using Confluent.Kafka;
using ProductService.Application.Abstractions;

namespace ProductService.Infrastructure.Message;

public class KafkaProducer : IMessageProvider
{
    private readonly IProducer<string, string> _producer;
    private string _topic = "product";

    public KafkaProducer(string bootstrapServers)
    {
        var config = new ProducerConfig { BootstrapServers = bootstrapServers };
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishAsync(string title, object payload)
    {
        var message = JsonSerializer.Serialize( new { title, payload });
        await _producer.ProduceAsync(_topic, new Message<string, string> { Key = Guid.NewGuid().ToString(), Value = message });

        _producer.Flush(TimeSpan.FromSeconds(10));
    }
}