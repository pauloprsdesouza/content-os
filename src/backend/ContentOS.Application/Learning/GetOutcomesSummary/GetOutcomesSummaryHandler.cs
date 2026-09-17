using ContentOS.Application.Learning.Ports;

namespace ContentOS.Application.Learning.GetOutcomesSummary;

public sealed class GetOutcomesSummaryHandler(IOutcomesSummaryQuery summaryQuery)
{
    public Task<OutcomesSummary> HandleAsync(CancellationToken cancellationToken = default) =>
        summaryQuery.GetAsync(cancellationToken);
}
