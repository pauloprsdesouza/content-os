using ContentOS.Application.Content.Ports;
using ContentOS.Application.Messaging;
using ContentOS.Application.Operations.Ports;
using ContentOS.Application.Persistence;
using ContentOS.Domain.Content;
using ContentOS.Domain.Operations;
using ContentOS.SharedKernel;

namespace ContentOS.Application.Content.CreateUnit;

public sealed class CreateContentUnitHandler(
    IContentUnitRepository units,
    IContentVersionRepository versions,
    IOperationRepository operations,
    IAiCommandPublisher aiCommands,
    IIdGenerator ids,
    TimeProvider clock,
    IChangeCommitter changes)
{
    public async Task<CreateContentUnitResult> HandleAsync(
        CreateContentUnitCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Title))
        {
            return CreateContentUnitResult.Invalid("CONTENT_TITLE_REQUIRED");
        }

        var now = clock.GetUtcNow();
        var unitId = ids.NewId();
        var versionId = ids.NewId();

        var unit = ContentUnit.Create(
            unitId,
            command.Title,
            command.Brief,
            command.CreatedByUserId,
            now);
        var version = ContentVersion.CreateDraft(
            versionId,
            unitId,
            revision: 1,
            bodyMarkdown: null,
            createdByUserId: command.CreatedByUserId,
            createdAt: now);

        Guid? operationId = null;
        if (command.QueueGeneration)
        {
            operationId = ids.NewId();
            version.BindOperation(operationId.Value, now);
            version.QueueGeneration(now);

            var operation = Operation.Create(
                operationId.Value,
                AiMessageTypes.ContentGenerate,
                "content-version",
                versionId,
                command.CreatedByUserId,
                now);
            operations.Add(operation);
        }

        units.Add(unit);
        versions.Add(version);

        if (command.QueueGeneration && operationId is not null)
        {
            await aiCommands.PublishAsync(
                new AiCloudEventEnvelope(
                    Id: ids.NewId().ToString("N"),
                    Source: "contentos.content",
                    Type: AiMessageTypes.ContentGenerate,
                    Time: now,
                    Subject: $"content-version/{versionId}",
                    CorrelationId: operationId.Value.ToString("N"),
                    IdempotencyKey: $"content-generate:{versionId}:v{version.Version}",
                    SchemaVersion: "1",
                    Data: new
                    {
                        contentUnitId = unitId,
                        contentVersionId = versionId,
                        operationId,
                        title = unit.Title,
                        brief = unit.Brief
                    }),
                cancellationToken);
        }

        await changes.CommitAsync(cancellationToken);

        return CreateContentUnitResult.Created(unitId, versionId, operationId);
    }
}
