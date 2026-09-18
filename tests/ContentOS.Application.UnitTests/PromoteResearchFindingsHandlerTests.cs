using ContentOS.Application.Knowledge.Claims.Promote;
using ContentOS.Domain.Knowledge;
using Shouldly;

namespace ContentOS.Application.UnitTests;

public sealed class PromoteResearchFindingsHandlerTests
{
    [Fact]
    public async Task Valid_finding_creates_one_pending_claim()
    {
        var claims = new InMemoryClaimRepository();
        var handler = CreateHandler(claims, snapshotsExist: true);
        var findingId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var first = await handler.HandleAsync(Command(findingId, withEvidence: true));
        var second = await handler.HandleAsync(Command(findingId, withEvidence: true));

        first.Promoted.ShouldBe(1);
        second.AlreadyPromoted.ShouldBe(1);
        claims.Items.Count.ShouldBe(1);
        claims.Items[0].Status.ShouldBe(ClaimStatus.PendingReview);
        claims.Items[0].OriginResearchFindingId.ShouldBe(findingId);
    }

    [Fact]
    public async Task Missing_snapshot_does_not_create_a_claim()
    {
        var claims = new InMemoryClaimRepository();
        var handler = CreateHandler(claims, snapshotsExist: false);

        var result = await handler.HandleAsync(Command(Guid.NewGuid(), withEvidence: false));

        result.NeedsEvidence.ShouldBe(1);
        result.Promoted.ShouldBe(0);
        claims.Items.ShouldBeEmpty();
    }

    private static PromoteResearchFindingsHandler CreateHandler(
        InMemoryClaimRepository claims,
        bool snapshotsExist) =>
        new(
            claims,
            new InMemoryEvidenceRepository(),
            new FixedSnapshotExistsQuery(snapshotsExist),
            new SequentialIdGenerator(),
            TimeProvider.System);

    private static PromoteResearchFindingsCommand Command(Guid findingId, bool withEvidence) =>
        new(
            Guid.NewGuid(),
            [
                new PromoteResearchFindingItem(
                    findingId,
                    "A claim needs a source.",
                    0.8m,
                    withEvidence ? Guid.Parse("22222222-2222-2222-2222-222222222222") : null,
                    withEvidence ? "page:1" : null,
                    "research-agent")
            ]);
}
