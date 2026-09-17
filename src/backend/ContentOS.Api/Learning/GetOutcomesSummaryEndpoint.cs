using ContentOS.Application.Learning.GetOutcomesSummary;
using ContentOS.Contracts.Learning;

namespace ContentOS.Api.Learning;

public static class GetOutcomesSummaryEndpoint
{
    public static RouteGroupBuilder MapGetOutcomesSummary(this RouteGroupBuilder group)
    {
        group.MapGet(
                "/summary",
                async (
                    GetOutcomesSummaryHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    var summary = await handler.HandleAsync(cancellationToken);
                    return Results.Ok(
                        new OutcomesSummaryResponse(
                            summary.ConfirmedPurchases,
                            summary.Learners,
                            summary.Enrollments,
                            summary.CapstonesSubmitted,
                            summary.EvaluationsPassed,
                            summary.OutcomesRecorded));
                })
            .RequireAuthorization();

        return group;
    }
}
