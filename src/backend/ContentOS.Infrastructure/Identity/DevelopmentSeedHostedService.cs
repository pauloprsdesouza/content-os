using System.Text;
using ContentOS.Application.Knowledge.Claims.CreateForReview;
using ContentOS.Application.Knowledge.Ports;
using ContentOS.Application.Knowledge.Snapshots.Create;
using ContentOS.Application.Knowledge.Sources.Create;
using ContentOS.Domain.Identity;
using ContentOS.Domain.Knowledge;
using ContentOS.Infrastructure.Options;
using ContentOS.SharedKernel;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ContentOS.Infrastructure.Identity;

public sealed class DevelopmentSeedHostedService(
    IServiceScopeFactory scopeFactory,
    IOptions<DevelopmentSeedOptions> options,
    IHostEnvironment environment,
    ILogger<DevelopmentSeedHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment() || !options.Value.Enabled)
        {
            return;
        }

        await using var scope = scopeFactory.CreateAsyncScope();
        var services = scope.ServiceProvider;
        await EnsureAdminAsync(services, cancellationToken);

        if (options.Value.SeedDemoKnowledge)
        {
            await EnsureDemoKnowledgeAsync(services, cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task EnsureAdminAsync(
        IServiceProvider services,
        CancellationToken cancellationToken)
    {
        var seed = options.Value;
        if (string.IsNullOrWhiteSpace(seed.AdminEmail)
            || string.IsNullOrWhiteSpace(seed.AdminPassword))
        {
            logger.LogWarning("Development seed is enabled but admin credentials are missing.");
            return;
        }

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        const string adminRole = "Admin";

        if (!await roleManager.RoleExistsAsync(adminRole))
        {
            var role = new IdentityRole<Guid>(adminRole)
            {
                Id = Guid.CreateVersion7()
            };
            var roleResult = await roleManager.CreateAsync(role);
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to create Admin role: {FormatErrors(roleResult)}");
            }
        }

        var user = await userManager.FindByEmailAsync(seed.AdminEmail);
        if (user is null)
        {
            user = new ApplicationUser
            {
                Id = Guid.CreateVersion7(),
                UserName = seed.AdminEmail,
                Email = seed.AdminEmail,
                EmailConfirmed = true
            };
            var createResult = await userManager.CreateAsync(user, seed.AdminPassword);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to create demo admin: {FormatErrors(createResult)}");
            }
        }

        if (!await userManager.IsInRoleAsync(user, adminRole))
        {
            var addRole = await userManager.AddToRoleAsync(user, adminRole);
            if (!addRole.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to assign Admin role: {FormatErrors(addRole)}");
            }
        }

        var claims = await userManager.GetClaimsAsync(user);
        if (!claims.Any(claim =>
                claim.Type == "capability"
                && claim.Value == CapabilityNames.KnowledgeApprove))
        {
            var addClaim = await userManager.AddClaimAsync(
                user,
                new System.Security.Claims.Claim("capability", CapabilityNames.KnowledgeApprove));
            if (!addClaim.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to add knowledge.approve claim: {FormatErrors(addClaim)}");
            }
        }

        claims = await userManager.GetClaimsAsync(user);
        if (!claims.Any(claim =>
                claim.Type == "capability"
                && claim.Value == CapabilityNames.ContentApprove))
        {
            var addClaim = await userManager.AddClaimAsync(
                user,
                new System.Security.Claims.Claim("capability", CapabilityNames.ContentApprove));
            if (!addClaim.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to add content.approve claim: {FormatErrors(addClaim)}");
            }
        }

        logger.LogInformation("Development admin seed ensured for {Email}.", seed.AdminEmail);
        cancellationToken.ThrowIfCancellationRequested();
    }

    private async Task EnsureDemoKnowledgeAsync(
        IServiceProvider services,
        CancellationToken cancellationToken)
    {
        var sourcesQuery = services.GetRequiredService<ISourcesQuery>();
        var existing = await sourcesQuery.GetPageAsync(1, 1, cancellationToken);
        if (existing.TotalItems > 0)
        {
            return;
        }

        var createSource = services.GetRequiredService<CreateSourceHandler>();
        var createSnapshot = services.GetRequiredService<CreateSnapshotHandler>();
        var createClaim = services.GetRequiredService<CreateClaimForReviewHandler>();

        var sourceResult = await createSource.HandleAsync(
            new CreateSourceCommand(
                "https://example.com/content-os/demo-source",
                SourceKind.Web,
                "Demo Source",
                IdempotencyKey.Create("seed-source")),
            cancellationToken);

        if (!sourceResult.IsSuccess || sourceResult.SourceId is null)
        {
            logger.LogWarning(
                "Skipped demo knowledge seed; create source failed with {Code}.",
                sourceResult.ErrorCode);
            return;
        }

        var payload = Encoding.UTF8.GetBytes(
            "Content OS demo snapshot used for local claim review.");
        await using var content = new MemoryStream(payload, writable: false);
        var snapshotResult = await createSnapshot.HandleAsync(
            new CreateSnapshotCommand(
                sourceResult.SourceId.Value,
                content,
                "text/plain",
                IdempotencyKey.Create("seed-snapshot")),
            cancellationToken);

        if (!snapshotResult.IsSuccess || snapshotResult.SnapshotId is null)
        {
            logger.LogWarning(
                "Skipped demo claim seed; create snapshot failed with {Code}.",
                snapshotResult.ErrorCode);
            return;
        }

        var claimResult = await createClaim.HandleAsync(
            new CreateClaimForReviewCommand(
                "Content OS preserves provenance from source bytes through approved claims.",
                0.92m,
                snapshotResult.SnapshotId.Value,
                "bytes=0-64",
                "manual-seed",
                IdempotencyKey.Create("seed-claim")),
            cancellationToken);

        if (claimResult.IsSuccess)
        {
            logger.LogInformation(
                "Seeded demo claim {ClaimId} for local review.",
                claimResult.ClaimId);
        }
    }

    private static string FormatErrors(IdentityResult result) =>
        string.Join("; ", result.Errors.Select(error => error.Description));
}
