using ContentOS.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace ContentOS.Infrastructure.Options;

public sealed class ProductionConfigurationGuard(
    IHostEnvironment environment,
    IOptions<CommerceOptions> commerce,
    IOptions<DevelopmentSeedOptions> seed,
    IOptions<RabbitMqOptions> rabbit,
    IConfiguration configuration) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!environment.IsProduction())
        {
            return Task.CompletedTask;
        }

        var failures = new List<string>();
        RejectPlaceholder(failures, "ConnectionStrings:Platform", configuration["ConnectionStrings:Platform"]);
        RejectPlaceholder(failures, "RabbitMq:Uri", rabbit.Value.Uri);
        if (seed.Value.Enabled)
        {
            failures.Add("DevelopmentSeed:Enabled must be false in production.");
        }

        if (commerce.Value.UseStubProvider)
        {
            failures.Add("Commerce:UseStubProvider must be false in production.");
        }

        if (string.IsNullOrWhiteSpace(commerce.Value.ApiKey)
            || commerce.Value.ApiKey.StartsWith("REPLACE_ME", StringComparison.OrdinalIgnoreCase))
        {
            failures.Add("Commerce:ApiKey is required in production.");
        }

        if (string.IsNullOrWhiteSpace(commerce.Value.ProductMappings))
        {
            failures.Add("Commerce:ProductMappings is required in production.");
        }

        if (failures.Count > 0)
        {
            throw new InvalidOperationException(
                "Production configuration rejected: " + string.Join(" ", failures));
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static void RejectPlaceholder(List<string> failures, string name, string? value)
    {
        if (string.IsNullOrWhiteSpace(value)
            || value.Contains("REPLACE_ME", StringComparison.OrdinalIgnoreCase)
            || value.Contains("ChangeMe", StringComparison.OrdinalIgnoreCase))
        {
            failures.Add($"{name} is missing or still a placeholder.");
        }
    }
}
