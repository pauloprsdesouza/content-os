using ContentOS.Application.Content.Formats;
using ContentOS.Application.Content.Literature;
using ContentOS.Application.Content.Ports;
using ContentOS.Application.Messaging;
using ContentOS.Application.Operations.Ports;
using ContentOS.Application.Persistence;
using ContentOS.Domain.Content;
using ContentOS.Domain.Operations;
using ContentOS.SharedKernel;

namespace ContentOS.Application.Content.Topics;

public sealed class StartTopicDiscoveryHandler(
    IScholarlyLiterature literature,
    ITopicDiscoveryRepository discoveries,
    IOperationRepository operations,
    IAiCommandPublisher aiCommands,
    IIdGenerator ids,
    TimeProvider clock,
    IChangeCommitter changes)
{
    public async Task<StartTopicDiscoveryResult> HandleAsync(
        StartTopicDiscoveryCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!ContentFormat.TryParse(command.Format, out var format))
        {
            return StartTopicDiscoveryResult.Invalid("CONTENT_FORMAT_UNKNOWN");
        }

        if (command.WindowDays is not (7 or 30 or 90))
        {
            return StartTopicDiscoveryResult.Invalid("TOPIC_WINDOW_INVALID");
        }

        if (string.IsNullOrWhiteSpace(command.AreaId) || string.IsNullOrWhiteSpace(command.AreaName))
        {
            return StartTopicDiscoveryResult.Invalid("TOPIC_AREA_REQUIRED");
        }

        if (command.SeriesId is Guid seriesId && command.ScheduledFor is DateTimeOffset scheduledFor)
        {
            var existing = await discoveries.FindScheduledAsync(seriesId, scheduledFor, cancellationToken);
            if (existing is not null)
            {
                return StartTopicDiscoveryResult.Duplicate(existing.Id);
            }
        }

        var search = await literature.SearchRecentWorksAsync(
            new ScholarlyWorkQuery(command.AreaId.Trim(), command.AreaIsSubfield, command.WindowDays),
            cancellationToken);
        if (!search.IsSuccess)
        {
            return StartTopicDiscoveryResult.Invalid(search.ErrorCode ?? "LITERATURE_UNAVAILABLE");
        }

        var now = clock.GetUtcNow();
        TopicDiscovery discovery;
        try
        {
            discovery = TopicDiscovery.Start(
                ids.NewId(),
                format,
                command.AreaId,
                command.AreaName,
                command.AreaIsSubfield,
                command.WindowDays,
                command.OwnerUserId,
                command.SeriesId,
                command.ScheduledFor,
                now);
        }
        catch (ArgumentException)
        {
            return StartTopicDiscoveryResult.Invalid("TOPIC_AREA_REQUIRED");
        }

        discoveries.Add(discovery);

        if (search.Items.Count == 0)
        {
            discovery.MarkAwaitingSelection(now);
            await changes.CommitAsync(cancellationToken);
            return StartTopicDiscoveryResult.Started(discovery.Id, null, isEmpty: true);
        }

        var works = new List<DiscoveredWork>();
        foreach (var item in search.Items)
        {
            works.Add(DiscoveredWork.Create(
                ids.NewId(),
                discovery.Id,
                item.WorkId,
                item.Title,
                item.PublicationYear,
                item.TopicId,
                item.TopicName,
                item.AbstractText));
        }

        discoveries.AddWorks(works);

        var operationId = ids.NewId();
        discovery.BindOperation(operationId, now);
        operations.Add(Operation.Create(
            operationId,
            AiMessageTypes.TopicLabel,
            "topic-discovery",
            discovery.Id,
            command.OwnerUserId,
            now));

        await aiCommands.PublishAsync(
            new AiCloudEventEnvelope(
                Id: ids.NewId().ToString("N"),
                Source: "contentos.content",
                Type: AiMessageTypes.TopicLabel,
                Time: now,
                Subject: $"topic-discovery/{discovery.Id}",
                CorrelationId: operationId.ToString("N"),
                IdempotencyKey: $"topic-label:{discovery.Id}",
                SchemaVersion: "1",
                Data: new
                {
                    discoveryId = discovery.Id,
                    operationId,
                    works = works.Select(work => new TopicLabelWork(
                        work.WorkId,
                        work.Title,
                        work.PublicationYear,
                        work.TopicName,
                        Truncate(work.AbstractText, 1500))).ToArray()
                }),
            cancellationToken);

        await changes.CommitAsync(cancellationToken);
        return StartTopicDiscoveryResult.Started(discovery.Id, operationId, isEmpty: false);
    }

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..max];
}
