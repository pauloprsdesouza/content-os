using ContentOS.Infrastructure.Options;
using ContentOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;

namespace ContentOS.Migrations;

public sealed class MigrationRunner(
    PlatformDbContext dbContext,
    IOptions<ConnectionStringsOptions> connectionStrings)
{
    private const long AdvisoryLockKey = 7307313578463756845;

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        await using var lockConnection =
            new NpgsqlConnection(connectionStrings.Value.Platform);
        await lockConnection.OpenAsync(cancellationToken);

        await ExecuteAsync(
            lockConnection,
            $"SELECT pg_advisory_lock({AdvisoryLockKey});",
            cancellationToken);

        try
        {
            await EnsureSchemasAsync(lockConnection, cancellationToken);
            await dbContext.Database.MigrateAsync(cancellationToken);
        }
        finally
        {
            await ExecuteAsync(
                lockConnection,
                $"SELECT pg_advisory_unlock({AdvisoryLockKey});",
                CancellationToken.None);
        }
    }

    private static async Task EnsureSchemasAsync(
        NpgsqlConnection connection,
        CancellationToken cancellationToken)
    {
        await ExecuteAsync(
            connection,
            "CREATE EXTENSION IF NOT EXISTS vector;",
            cancellationToken);

        foreach (var schema in ModuleSchemas.All)
        {
            await ExecuteAsync(
                connection,
                $"CREATE SCHEMA IF NOT EXISTS \"{schema}\";",
                cancellationToken);
        }
    }

    private static async Task ExecuteAsync(
        NpgsqlConnection connection,
        string commandText,
        CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(commandText, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
