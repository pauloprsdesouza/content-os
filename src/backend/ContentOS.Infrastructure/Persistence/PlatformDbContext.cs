using ContentOS.Domain.Catalog;
using ContentOS.Domain.Commerce;
using ContentOS.Domain.Content;
using ContentOS.Domain.Knowledge;
using ContentOS.Domain.Learning;
using ContentOS.Domain.Operations;
using ContentOS.Domain.Publication;
using ContentOS.Domain.Research;
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

    public DbSet<ResearchJob> ResearchJobs => Set<ResearchJob>();

    public DbSet<ResearchFinding> ResearchFindings => Set<ResearchFinding>();

    public DbSet<ContentUnit> ContentUnits => Set<ContentUnit>();

    public DbSet<ContentVersion> ContentVersions => Set<ContentVersion>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Edition> Editions => Set<Edition>();

    public DbSet<CurriculumItem> CurriculumItems => Set<CurriculumItem>();

    public DbSet<PublicationPackage> PublicationPackages => Set<PublicationPackage>();

    public DbSet<WebhookInboxEntry> WebhookInboxEntries => Set<WebhookInboxEntry>();

    public DbSet<Purchase> Purchases => Set<Purchase>();

    public DbSet<ReconciliationRun> ReconciliationRuns => Set<ReconciliationRun>();

    public DbSet<Learner> Learners => Set<Learner>();

    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    public DbSet<Capstone> Capstones => Set<Capstone>();

    public DbSet<Evaluation> Evaluations => Set<Evaluation>();

    public DbSet<Outcome> Outcomes => Set<Outcome>();

    public DbSet<Operation> Operations => Set<Operation>();
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
