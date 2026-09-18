using ContentOS.Application.Content.Formats;
using ContentOS.Application.Content.Generation;
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
    ContentFormatStrategyCatalog formats,
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

        if (!ContentFormat.TryParse(command.Format, out var format))
        {
            return CreateContentUnitResult.Invalid("CONTENT_FORMAT_UNKNOWN");
        }

        var strategy = formats.Find(format);
        if (strategy is null)
        {
            return CreateContentUnitResult.Invalid("CONTENT_FORMAT_UNKNOWN");
        }

        var now = clock.GetUtcNow();
        var unitId = ids.NewId();
        var versionId = ids.NewId();

        ContentUnit unit;
        try
        {
            unit = ContentUnit.Create(
                unitId,
                command.Title,
                command.Brief,
                format,
                command.CitationContentHashes,
                command.TopicDiscoveryId,
                command.ProductId,
                command.CreatedByUserId,
                now);
        }
        catch (ArgumentException)
        {
            return CreateContentUnitResult.Invalid("CONTENT_CITATION_INVALID");
        }
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
                    Data: new ContentGenerationBrief(
                        unitId,
                        versionId,
                        operationId.Value,
                        unit.Title,
                        unit.Brief,
                        unit.Format.Code,
                        strategy.BuildMold(),
                        unit.ListCitationHashes(),
                        false)),
                cancellationToken);
        }

        await changes.CommitAsync(cancellationToken);

        return CreateContentUnitResult.Created(unitId, versionId, operationId);
    }
}
