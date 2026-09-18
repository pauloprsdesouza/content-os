namespace ContentOS.Infrastructure.Options;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string Uri { get; init; } = string.Empty;
    public string AiRequestsExchange { get; init; } = string.Empty;
    public string AiResultsQueue { get; init; } = string.Empty;
    public string AiResultsDeadLetterQueue { get; init; } = string.Empty;
    public int AiResultMaxAttempts { get; init; } = 5;
}
