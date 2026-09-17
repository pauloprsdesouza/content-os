using ContentOS.SharedKernel;

namespace ContentOS.Application.Knowledge.Snapshots.Create;

public sealed record CreateSnapshotCommand(
    Guid SourceId,
    Stream Content,
    string MediaType,
    IdempotencyKey IdempotencyKey);
