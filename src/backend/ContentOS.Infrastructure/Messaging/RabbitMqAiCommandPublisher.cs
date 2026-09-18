using System.Text;
using ContentOS.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace ContentOS.Infrastructure.Messaging;

public sealed class RabbitMqAiCommandPublisher(
    IOptions<RabbitMqOptions> options,
    ILogger<RabbitMqAiCommandPublisher> logger) : IAsyncDisposable
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private IConnection? _connection;
    private IChannel? _channel;

    public async Task PublishRawAsync(
        string messageType,
        string envelopeId,
        string payloadJson,
        CancellationToken cancellationToken = default)
    {
        var rabbit = options.Value;
        if (string.IsNullOrWhiteSpace(rabbit.Uri)
            || string.IsNullOrWhiteSpace(rabbit.AiRequestsExchange))
        {
            throw new InvalidOperationException("RabbitMQ Uri/exchange is not configured.");
        }

        await _gate.WaitAsync(cancellationToken);
        try
        {
            await EnsureChannelAsync(cancellationToken);
            var body = Encoding.UTF8.GetBytes(payloadJson);
            var properties = new BasicProperties
            {
                ContentType = "application/cloudevents+json",
                DeliveryMode = DeliveryModes.Persistent,
                MessageId = envelopeId,
                Type = messageType
            };

            await _channel!.BasicPublishAsync(
                exchange: rabbit.AiRequestsExchange,
                routingKey: messageType,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);

            logger.LogInformation("Dispatched outbox message {MessageType} {EnvelopeId}", messageType, envelopeId);
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
