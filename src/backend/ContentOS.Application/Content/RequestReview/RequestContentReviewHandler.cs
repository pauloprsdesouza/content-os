using ContentOS.Application.Content.Ports;
using ContentOS.Application.Messaging;
using ContentOS.Application.Operations.Ports;
using ContentOS.Application.Persistence;
using ContentOS.Domain.Content;
using ContentOS.Domain.Operations;
using ContentOS.SharedKernel;

namespace ContentOS.Application.Content.RequestReview;

public sealed class RequestContentReviewHandler(
    IContentVersionRepository versions,
    IOperationRepository operations,
    IAiCommandPublisher aiCommands,
    IIdGenerator ids,
    TimeProvider clock,
    IChangeCommitter changes)
{
    public async Task<RequestContentReviewResult> HandleAsync(
        RequestContentReviewCommand command,
        CancellationToken cancellationToken = default)
    {
        var version = await versions.GetByIdAsync(command.ContentVersionId, cancellationToken);
        if (version is null)
        {
            return RequestContentReviewResult.NotFound();
        }

        var now = clock.GetUtcNow();
        var operationId = ids.NewId();

        try
        {
            if ((version.Status is ContentVersionStatus.Draft or ContentVersionStatus.ChangesRequested)
                && string.IsNullOrWhiteSpace(version.BodyMarkdown))
            {
                version.BindOperation(operationId, now);
                version.QueueGeneration(now);
                operations.Add(Operation.Create(
                    operationId,
                    AiMessageTypes.ContentGenerate,
                    "content-version",
                    version.Id,
                    command.RequestedByUserId,
                    now));
                await aiCommands.PublishAsync(
                    new AiCloudEventEnvelope(
                        Id: ids.NewId().ToString("N"),
                        Source: "contentos.content",
                        Type: AiMessageTypes.ContentGenerate,
                        Time: now,
                        Subject: $"content-version/{version.Id}",
                        CorrelationId: operationId.ToString("N"),
                        IdempotencyKey: $"content-generate:{version.Id}:v{version.Version}",
                        SchemaVersion: "1",
                        Data: new
                        {
                            contentUnitId = version.ContentUnitId,
                            contentVersionId = version.Id,
                            operationId,
                            requestReviewAfterGenerate = true
                        }),
                    cancellationToken);
                await changes.CommitAsync(cancellationToken);

                return RequestContentReviewResult.Accepted(operationId);
            }

            if (version.Status == ContentVersionStatus.Generated)
            {
                version.BindOperation(operationId, now);
                version.QueueAgentReview(now);
                operations.Add(Operation.Create(
                    operationId,
                    AiMessageTypes.ContentReview,
                    "content-version",
                    version.Id,
                    command.RequestedByUserId,
                    now));
                await aiCommands.PublishAsync(
                    new AiCloudEventEnvelope(
                        Id: ids.NewId().ToString("N"),
                        Source: "contentos.content",
                        Type: AiMessageTypes.ContentReview,
                        Time: now,
                        Subject: $"content-version/{version.Id}",
                        CorrelationId: operationId.ToString("N"),
                        IdempotencyKey: $"content-review:{version.Id}:v{version.Version}",
                        SchemaVersion: "1",
                        Data: new
                        {
                            contentVersionId = version.Id,
                            operationId,
                            bodyMarkdown = version.BodyMarkdown
                        }),
                    cancellationToken);
                await changes.CommitAsync(cancellationToken);

                return RequestContentReviewResult.Accepted(operationId);
            }

            if (version.Status is ContentVersionStatus.Draft
                or ContentVersionStatus.ChangesRequested
                or ContentVersionStatus.AgentReviewed)
            {
                version.RequestHumanApproval(now);
                var operation = Operation.Create(
                    operationId,
                    "content.human-review",
                    "content-version",
                    version.Id,
                    command.RequestedByUserId,
                    now);
                operation.MarkSucceeded(now);
                operations.Add(operation);
                await changes.CommitAsync(cancellationToken);
                return RequestContentReviewResult.Accepted(operationId);
            }

            return RequestContentReviewResult.Invalid("CONTENT_REVIEW_INVALID_STATE");
        }
        catch (InvalidOperationException)
        {
            return RequestContentReviewResult.Invalid("CONTENT_REVIEW_INVALID_STATE");
        }
    }
}
