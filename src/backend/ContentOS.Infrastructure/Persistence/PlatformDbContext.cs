using ContentOS.Domain.Knowledge;
using ContentOS.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ContentOS.Infrastructure.Persistence;

public sealed class PlatformDbContext(
    DbContextOptions<PlatformDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Source> Sources => Set<Source>();

    public DbSet<SourceSnapshot> SourceSnapshots => Set<SourceSnapshot>();

    public DbSet<Evidence> Evidence => Set<Evidence>();

    public DbSet<Claim> Claims => Set<Claim>();

    public DbSet<ClaimEvidenceLink> ClaimEvidenceLinks => Set<ClaimEvidenceLink>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("vector");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PlatformDbContext).Assembly);

        modelBuilder.Entity<ApplicationUser>().ToTable("users", ModuleSchemas.Identity);
        modelBuilder.Entity<IdentityRole<Guid>>().ToTable("roles", ModuleSchemas.Identity);
        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("user_claims", ModuleSchemas.Identity);
        modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("user_roles", ModuleSchemas.Identity);
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("user_logins", ModuleSchemas.Identity);
        modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("role_claims", ModuleSchemas.Identity);
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("user_tokens", ModuleSchemas.Identity);
    }
}
