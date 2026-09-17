using ContentOS.Application.Messaging;
using ContentOS.Application.Operations.Ports;
using ContentOS.Application.Persistence;
using ContentOS.Application.Research.Ports;
using ContentOS.Domain.Operations;
using ContentOS.Domain.Research;
using ContentOS.SharedKernel;

namespace ContentOS.Application.Research.Create;

public sealed class CreateResearchJobHandler(
    IResearchJobRepository jobs,
    IOperationRepository operations,
    IAiCommandPublisher aiCommands,
    IIdGenerator ids,
    TimeProvider clock,
    IChangeCommitter changes)
{
    public async Task<CreateResearchJobResult> HandleAsync(
        CreateResearchJobCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Topic))
        {
            return CreateResearchJobResult.Invalid("RESEARCH_TOPIC_REQUIRED");
        }

        var now = clock.GetUtcNow();
        var jobId = ids.NewId();
        var operationId = ids.NewId();

        var job = ResearchJob.Create(
            jobId,
            command.Topic,
            command.ScopeNotes,
            command.RequestedByUserId,
            now);
        job.BindOperation(operationId, now);
        job.MarkQueued(now);

        var operation = Operation.Create(
            operationId,
            AiMessageTypes.Research,
            "research-job",
            jobId,
            command.RequestedByUserId,
            now);

        jobs.Add(job);
        operations.Add(operation);
        await changes.CommitAsync(cancellationToken);

        var envelope = new AiCloudEventEnvelope(
            Id: ids.NewId().ToString("N"),
            Source: "contentos.api",
            Type: AiMessageTypes.Research,
            Time: now,
            Subject: $"research-job/{jobId}",
            CorrelationId: operationId.ToString("N"),
            IdempotencyKey: $"research:{jobId}:attempt:{job.AttemptCount}",
            SchemaVersion: "1",
            Data: new
            {
                researchJobId = jobId,
                operationId,
                topic = job.Topic,
                scopeNotes = job.ScopeNotes
            });

        await aiCommands.PublishAsync(envelope, cancellationToken);

        return CreateResearchJobResult.Created(jobId, operationId);
    }
}
