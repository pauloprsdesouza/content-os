using ContentOS.Infrastructure.Options;
using ContentOS.Infrastructure.Persistence;
using ContentOS.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddOptions<ConnectionStringsOptions>()
    .Bind(builder.Configuration.GetSection(ConnectionStringsOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.Platform))
    .ValidateOnStart();
builder.Services.AddDbContext<PlatformDbContext>((serviceProvider, options) =>
{
    var connectionString = serviceProvider
        .GetRequiredService<IOptions<ConnectionStringsOptions>>()
        .Value
        .Platform;
    options.UseNpgsql(connectionString);
});
builder.Services.AddScoped<MigrationRunner>();

using var host = builder.Build();
await using var scope = host.Services.CreateAsyncScope();
var runner = scope.ServiceProvider.GetRequiredService<MigrationRunner>();
await runner.RunAsync(CancellationToken.None);
