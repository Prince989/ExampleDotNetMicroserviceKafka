using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SearchService.Application.Abstractions;
using SearchService.Domain.Entities;

public sealed class KafkaConsumer : BackgroundService
{
    private readonly ISearchRepository _repo;
    private readonly IConsumer<string, string> _consumer;
    private readonly ILogger<KafkaConsumer> _logger;
    private static readonly string[] Topics = new[] { "product", "order" };

    private readonly Dictionary<string, Func<string, Task>> _topicHandlers;

    public KafkaConsumer(
        ISearchRepository repo,
        string bootstrapServers, 
        ILogger<KafkaConsumer> logger,
        string groupId = "search-service")
    {
        _repo = repo;
        _logger = logger;
        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };
        _consumer = new ConsumerBuilder<string, string>(config).Build();

        _topicHandlers = new()
        {
            ["product"] = HandleProductRawAsync,
            ["order"]   = HandleOrderRawAsync
        };
    }
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(Topics);
        _logger.LogInformation("KafkaConsumer started. Subscribed to: {Topics}", string.Join(",", Topics));

        return Task.Run(async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var cr = _consumer.Consume(TimeSpan.FromSeconds(1));
                    if (cr == null)
                    {
                        _logger.LogDebug("Kafka poll heartbeat...");
                        continue;
                    }

                    if (cr.Message == null || string.IsNullOrWhiteSpace(cr.Message.Value))
                        continue;

                    if (_topicHandlers.TryGetValue(cr.Topic, out var handler))
                        await handler(cr.Message.Value);
                    else
                        _logger.LogWarning("No handler for topic {Topic}", cr.Topic);
                }
                catch (OperationCanceledException)
                {
                    /* shutting down */
                }
                catch (ConsumeException cex)
                {
                    _logger.LogError(cex, "Kafka consume error: {Reason}", cex.Error.Reason);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in KafkaConsumer loop");
                }
            }
            _logger.LogInformation("KafkaConsumer stopping...");
        }, stoppingToken);
    }
    private async Task HandleProductRawAsync(string raw)
    {
        var envelope = DeserializeEnvelope(raw);
        Console.WriteLine(raw);
        Console.WriteLine(envelope);
        if (envelope is null) return;

        switch (envelope.Title)
        {
            case "product.created":
            case "product.updated":
                Console.WriteLine("I came here");
                var prod = TryDeserialize<ProductDocument>(envelope.Payload);
                if (prod is not null)
                    await _repo.IndexAsync(prod);
                break;

            case "product.deleted":
                if (TryGetId(envelope.Payload, out var pid))
                    await _repo.DeleteAsync<ProductDocument>(pid);
                break;
        }
    }

    private async Task HandleOrderRawAsync(string raw)
    {
        var envelope = DeserializeEnvelope(raw);
        Console.WriteLine(raw);
        Console.WriteLine(envelope);
        if (envelope is null) return;

        switch (envelope.Title)
        {
            case "order.created":
                var ord = TryDeserialize<OrderDocument>(envelope.Payload);
                if (ord is not null)
                    await _repo.IndexAsync(ord);
                break;
        }
    }
    
    private static KafkaEvent<JsonElement>? DeserializeEnvelope(string raw)
    {
        try
        {
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var env = JsonSerializer.Deserialize<KafkaEvent<JsonElement>>(raw, opts);
            if (env is null || string.IsNullOrWhiteSpace(env.Title)) return null;
            if (env.Payload.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null) return null;
            return env;
        }
        catch { return null; }
    }

    private static T? TryDeserialize<T>(JsonElement el)
    {
        try
        {
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return el.Deserialize<T>(opts);
        }
        catch { return default; }
    }

    private static bool TryGetId(JsonElement el, out string id)
    {
        id = string.Empty;
        if (el.ValueKind != JsonValueKind.Object) return false;
        if (!el.TryGetProperty("id", out var idEl)) return false;
        var s = idEl.GetString();
        if (string.IsNullOrWhiteSpace(s)) return false;
        id = s!;
        return true;
    }

    public override void Dispose()
    {
        _consumer.Close();
        _consumer.Dispose();
        base.Dispose();
    }
}

public sealed class KafkaEvent<T>
{
    public string Title { get; set; } = string.Empty;
    public T Payload { get; set; }
}
