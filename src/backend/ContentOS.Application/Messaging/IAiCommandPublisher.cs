namespace ContentOS.Application.Messaging;

public interface IAiCommandPublisher
{
    Task PublishAsync(
        AiCloudEventEnvelope envelope,
        CancellationToken cancellationToken = default);
}
