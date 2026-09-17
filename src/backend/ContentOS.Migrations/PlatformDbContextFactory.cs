using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ContentOS.Migrations;

public sealed class PlatformDbContextFactory : IDesignTimeDbContextFactory<PlatformDbContext>
{
    public PlatformDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("CONTENTOS_PLATFORM_CONNECTION")
            ?? "Host=127.0.0.1;Port=5432;Database=contentos;Username=contentos;Password=contentos";

        var options = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsAssembly("ContentOS.Migrations"))
            .Options;

        return new PlatformDbContext(options);
    }
}
