using ContentOS.Domain.Knowledge;
using ContentOS.SharedKernel;

namespace ContentOS.Application.Knowledge.Sources.Create;

public sealed record CreateSourceCommand(
    string Location,
    SourceKind Kind,
    string DisplayName,
    IdempotencyKey IdempotencyKey);
