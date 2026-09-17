using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ContentOS.Application.Messaging;
using ContentOS.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace ContentOS.Infrastructure.Messaging;

public sealed class RabbitMqAiCommandPublisher(
    IOptions<RabbitMqOptions> options,
    ILogger<RabbitMqAiCommandPublisher> logger) : IAiCommandPublisher, IAsyncDisposable
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly SemaphoreSlim _gate = new(1, 1);
    private IConnection? _connection;
    private IChannel? _channel;

    public async Task PublishAsync(
        AiCloudEventEnvelope envelope,
        CancellationToken cancellationToken = default)
    {
        var rabbit = options.Value;
        if (string.IsNullOrWhiteSpace(rabbit.Uri)
            || string.IsNullOrWhiteSpace(rabbit.AiRequestsExchange))
        {
            logger.LogWarning(
                "RabbitMQ AI publish skipped for {Type}; Uri/exchange not configured.",
                envelope.Type);
            return;
        }

        await _gate.WaitAsync(cancellationToken);
        try
        {
            await EnsureChannelAsync(cancellationToken);

            var payload = new Dictionary<string, object?>
            {
                ["specversion"] = "1.0",
                ["id"] = envelope.Id,
                ["source"] = envelope.Source,
                ["type"] = envelope.Type,
                ["time"] = envelope.Time,
                ["subject"] = envelope.Subject,
                ["correlationid"] = envelope.CorrelationId,
                ["idempotencykey"] = envelope.IdempotencyKey,
                ["schemaversion"] = envelope.SchemaVersion,
                ["data"] = envelope.Data
            };

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload, SerializerOptions));
            var properties = new BasicProperties
            {
                ContentType = "application/cloudevents+json",
                DeliveryMode = DeliveryModes.Persistent,
                MessageId = envelope.Id,
                CorrelationId = envelope.CorrelationId,
                Type = envelope.Type
            };

            await _channel!.BasicPublishAsync(
                exchange: rabbit.AiRequestsExchange,
                routingKey: envelope.Type,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);

            logger.LogInformation(
                "Published AI command {Type} subject {Subject}",
                envelope.Type,
                envelope.Subject);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task EnsureChannelAsync(CancellationToken cancellationToken)
    {
        if (_channel is { IsOpen: true })
        {
            return;
        }

        var factory = new ConnectionFactory
        {
            Uri = new Uri(options.Value.Uri)
        };

        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await _channel.ExchangeDeclareAsync(
            exchange: options.Value.AiRequestsExchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
        {
            await _channel.CloseAsync();
            await _channel.DisposeAsync();
        }

        if (_connection is not null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }

        _gate.Dispose();
    }
}
