using ContentOS.Application.Catalog.Ports;
using ContentOS.Application.Persistence;
using ContentOS.Application.Publication.Ports;

namespace ContentOS.Application.Catalog.ReplaceCurriculum;

public sealed class ReplaceCurriculumHandler(
    IEditionRepository editions,
    IApprovedContentVersionLookup approvedVersions,
    TimeProvider clock,
    IChangeCommitter changes)
{
    public async Task<ReplaceCurriculumResult> HandleAsync(
        ReplaceCurriculumCommand command,
        CancellationToken cancellationToken = default)
    {
        var edition = await editions.GetByIdAsync(command.EditionId, cancellationToken);
        if (edition is null)
        {
            return ReplaceCurriculumResult.NotFound();
        }

        if (edition.Version != command.ExpectedVersion)
        {
            return ReplaceCurriculumResult.ConcurrencyConflict(edition.Version);
        }

        if (command.ContentVersionIds.Count > 0)
        {
            var approved = await approvedVersions.GetApprovedAsync(
                command.ContentVersionIds,
                cancellationToken);

            if (approved.Count != command.ContentVersionIds.Distinct().Count())
            {
                return ReplaceCurriculumResult.InvalidCurriculum(
                    "CURRICULUM_REQUIRES_APPROVED_VERSIONS");
            }

            foreach (var versionId in command.ContentVersionIds)
            {
                if (!approved.ContainsKey(versionId))
                {
                    return ReplaceCurriculumResult.InvalidCurriculum(
                        "CURRICULUM_REQUIRES_APPROVED_VERSIONS");
                }
            }
        }

        try
        {
            edition.ReplaceCurriculum(
                command.ContentVersionIds,
                command.ExpectedVersion,
                clock.GetUtcNow());
        }
        catch (InvalidOperationException) when (edition.Version != command.ExpectedVersion)
        {
            return ReplaceCurriculumResult.ConcurrencyConflict(edition.Version);
        }
        catch (InvalidOperationException)
        {
            return ReplaceCurriculumResult.InvalidCurriculum("CURRICULUM_INVALID");
        }
        catch (ArgumentException)
        {
            return ReplaceCurriculumResult.InvalidCurriculum("CURRICULUM_INVALID");
        }

        await changes.CommitAsync(cancellationToken);
        return ReplaceCurriculumResult.Replaced(edition.Version);
    }
}
