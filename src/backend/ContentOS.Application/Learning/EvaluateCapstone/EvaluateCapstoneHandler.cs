using ContentOS.Application.Learning.Ports;
using ContentOS.Application.Persistence;
using ContentOS.Domain.Learning;
using ContentOS.SharedKernel;

namespace ContentOS.Application.Learning.EvaluateCapstone;

public sealed class EvaluateCapstoneHandler(
    ICapstoneRepository capstones,
    IEnrollmentRepository enrollments,
    IEvaluationRepository evaluations,
    IOutcomeRepository outcomes,
    IIdGenerator ids,
    TimeProvider clock,
    IChangeCommitter changes)
{
    public async Task<EvaluateCapstoneResult> HandleAsync(
        EvaluateCapstoneCommand command,
        CancellationToken cancellationToken = default)
    {
        var capstone = await capstones.GetByIdAsync(command.CapstoneId, cancellationToken);
        if (capstone is null)
        {
            return EvaluateCapstoneResult.NotFound();
        }

        var enrollment = await enrollments.GetByIdAsync(capstone.EnrollmentId, cancellationToken);
        if (enrollment is null)
        {
            return EvaluateCapstoneResult.NotFound();
        }

        var now = clock.GetUtcNow();
        try
        {
            capstone.MarkEvaluated(now);
        }
        catch (InvalidOperationException) when (capstone.Status != CapstoneStatus.Submitted)
        {
            return EvaluateCapstoneResult.InvalidState();
        }

        var evaluation = Evaluation.Create(
            ids.NewId(),
            capstone.Id,
            command.Passed,
            command.Score,
            now);
        evaluations.Add(evaluation);

        var outcomeId = ids.NewId();
        outcomes.Add(
            Outcome.Record(
                outcomeId,
                enrollment.LearnerId,
                enrollment.Id,
                capstone.Id,
                command.Passed,
                now));

        await changes.CommitAsync(cancellationToken);
        return EvaluateCapstoneResult.Evaluated(outcomeId);
    }
}
