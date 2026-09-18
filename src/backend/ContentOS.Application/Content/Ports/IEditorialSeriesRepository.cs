using ContentOS.Domain.Content;

namespace ContentOS.Application.Content.Ports;

public interface IEditorialSeriesRepository
{
    void Add(EditorialSeries series);

    Task<EditorialSeries?> GetByIdAsync(Guid seriesId, CancellationToken cancellationToken = default);

    void Remove(EditorialSeries series);

    Task<IReadOnlyList<EditorialSeries>> ListForOwnerAsync(
        Guid ownerUserId,
        CancellationToken cancellationToken = default);
}
