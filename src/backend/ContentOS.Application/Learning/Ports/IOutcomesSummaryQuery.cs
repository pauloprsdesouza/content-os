namespace ContentOS.Application.Learning.Ports;

public interface IOutcomesSummaryQuery
{
    Task<OutcomesSummary> GetAsync(CancellationToken cancellationToken = default);
}
