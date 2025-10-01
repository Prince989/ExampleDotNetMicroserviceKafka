using System.Text.Json;
using Confluent.Kafka;
using OrderService.Application.Abstractions;

namespace OrderService.Infrastructure.Message;

public class KafkaProducer : IMessageProvider
{
    private readonly IProducer<string, string> _producer;
    private readonly string _topic = "order";
    
    public KafkaProducer(string bootstrapServers)
    {
        var config = new ProducerConfig { BootstrapServers = bootstrapServers };
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task ProduceAsync(string title, object payload)
    {
        var message = JsonSerializer.Serialize(new { title, payload });
        await _producer.ProduceAsync(_topic, new Message<string, string>{ Key = Guid.NewGuid().ToString(), Value = message});
        _producer.Flush(TimeSpan.FromSeconds(10));
    }
    
}