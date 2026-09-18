using ContentOS.Domain.Content;

namespace ContentOS.Application.Content.Formats;

public interface IContentFormatStrategy
{
    ContentFormat Format { get; }

    string DisplayName { get; }

    string BuildMold();
}
