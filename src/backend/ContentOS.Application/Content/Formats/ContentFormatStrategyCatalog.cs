using ContentOS.Domain.Content;

namespace ContentOS.Application.Content.Formats;

public sealed class ContentFormatStrategyCatalog(IEnumerable<IContentFormatStrategy> strategies)
{
    public IContentFormatStrategy? Find(ContentFormat format) =>
        strategies.FirstOrDefault(strategy => strategy.Format.Equals(format));
}
