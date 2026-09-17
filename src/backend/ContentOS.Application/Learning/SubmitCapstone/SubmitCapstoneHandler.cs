using ContentOS.Application.Learning.Ports;
using ContentOS.Application.Persistence;
using ContentOS.Domain.Learning;

namespace ContentOS.Application.Learning.SubmitCapstone;

public sealed class SubmitCapstoneHandler(
    ICapstoneRepository capstones,
    TimeProvider clock,
    IChangeCommitter changes)
{
    public async Task<SubmitCapstoneResult> HandleAsync(
        SubmitCapstoneCommand command,
        CancellationToken cancellationToken = default)
    {
        var capstone = await capstones.GetByEnrollmentIdAsync(
            command.EnrollmentId,
            cancellationToken);
        if (capstone is null)
        {
            return SubmitCapstoneResult.NotFound();
        }

        try
        {
            capstone.Submit(clock.GetUtcNow());
        }
        catch (InvalidOperationException) when (capstone.Status != CapstoneStatus.Open)
        {
            return SubmitCapstoneResult.InvalidState();
        }

        await changes.CommitAsync(cancellationToken);
        return SubmitCapstoneResult.Submitted(capstone.Id);
    }
}
