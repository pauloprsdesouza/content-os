using ContentOS.Application.Blobs;
using ContentOS.Application.Catalog.GetEdition;
using ContentOS.Application.Catalog.ListProducts;
using ContentOS.Application.Catalog.Ports;
using ContentOS.Application.Catalog.ReplaceCurriculum;
using ContentOS.Application.Commerce.ListPurchases;
using ContentOS.Application.Commerce.Ports;
using ContentOS.Application.Commerce.ReceiveKiwifyWebhook;
using ContentOS.Application.Commerce.RunReconciliation;
using ContentOS.Application.Content.Approve;
using ContentOS.Application.Content.ApplyGenerateResult;
using ContentOS.Application.Content.ApplyReviewResult;
using ContentOS.Application.Content.CreateUnit;
using ContentOS.Application.Content.GetVersion;
using ContentOS.Application.Content.ListUnits;
using ContentOS.Application.Content.Ports;
using ContentOS.Application.Content.RequestChanges;
using ContentOS.Application.Content.RequestReview;
using ContentOS.Application.Content.UpdateVersion;
using ContentOS.Application.Knowledge.Claims.Approve;
using ContentOS.Application.Knowledge.Claims.CreateForReview;
using ContentOS.Application.Knowledge.Claims.GetClaim;
using ContentOS.Application.Knowledge.Claims.GetReviewQueue;
using ContentOS.Application.Knowledge.Claims.Promote;
using ContentOS.Application.Knowledge.Claims.Reject;
using ContentOS.Application.Knowledge.Ports;
using ContentOS.Application.Knowledge.Snapshots.Create;
using ContentOS.Application.Knowledge.Snapshots.GetSnapshots;
using ContentOS.Application.Knowledge.Sources.Create;
using ContentOS.Application.Knowledge.Sources.GetSource;
using ContentOS.Application.Knowledge.Sources.GetSources;
using ContentOS.Application.Learning.EvaluateCapstone;
using ContentOS.Application.Learning.GetOutcomesSummary;
using ContentOS.Application.Learning.Ports;
using ContentOS.Application.Learning.SubmitCapstone;
using ContentOS.Application.Messaging;
using ContentOS.Application.Operations.Get;
using ContentOS.Application.Operations.Ports;
using ContentOS.Application.Persistence;
using ContentOS.Application.Publication.ConfirmPublication;
using ContentOS.Application.Publication.CreatePackage;
using ContentOS.Application.Publication.ExportPackage;
using ContentOS.Application.Publication.GetPackage;
using ContentOS.Application.Publication.Ports;
using ContentOS.Application.Research.ApplyResult;
using ContentOS.Application.Research.Create;
using ContentOS.Application.Research.Get;
using ContentOS.Application.Research.List;
using ContentOS.Application.Research.Ports;
using ContentOS.Infrastructure.Blobs;
using ContentOS.Infrastructure.Catalog;
using ContentOS.Infrastructure.Commerce;
using ContentOS.Infrastructure.Content;
using ContentOS.Infrastructure.Ids;
using ContentOS.Application.Dashboard;
using ContentOS.Infrastructure.Dashboard;
using ContentOS.Infrastructure.Identity;
using ContentOS.Infrastructure.Knowledge;
using ContentOS.Infrastructure.Learning;
using ContentOS.Infrastructure.Messaging;
using ContentOS.Infrastructure.Operations;
using ContentOS.Infrastructure.Options;
using ContentOS.Infrastructure.Persistence;
using ContentOS.Infrastructure.Publication;
using ContentOS.Infrastructure.Research;
using ContentOS.SharedKernel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ContentOS.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddContentOSInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<ConnectionStringsOptions>()
            .Bind(configuration.GetSection(ConnectionStringsOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.Platform))
            .ValidateOnStart();
        services.AddOptions<BlobStoreOptions>()
            .Bind(configuration.GetSection(BlobStoreOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.RootPath))
            .ValidateOnStart();
        services.AddOptions<RabbitMqOptions>()
            .Bind(configuration.GetSection(RabbitMqOptions.SectionName));
        services.AddOptions<AiWorkerOptions>()
            .Bind(configuration.GetSection(AiWorkerOptions.SectionName));
        services.AddOptions<SecurityOptions>()
            .Bind(configuration.GetSection(SecurityOptions.SectionName));
        services.AddOptions<CommerceOptions>()
            .Bind(configuration.GetSection(CommerceOptions.SectionName));

        services.AddOptions<DevelopmentSeedOptions>()
            .Bind(configuration.GetSection(DevelopmentSeedOptions.SectionName));

        services.AddHttpClient(HttpKiwifyOrderReconciler.HttpClientName, (sp, client) =>
        {
            var commerce = sp.GetRequiredService<IOptions<CommerceOptions>>().Value;
            if (!string.IsNullOrWhiteSpace(commerce.BaseUrl)
                && Uri.TryCreate(commerce.BaseUrl, UriKind.Absolute, out var baseUri))
            {
                client.BaseAddress = baseUri;
            }

            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddDbContext<PlatformDbContext>((serviceProvider, options) =>
        {
            var connectionString = serviceProvider
                .GetRequiredService<IOptions<ConnectionStringsOptions>>()
                .Value
                .Platform;
            options.UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsAssembly("ContentOS.Migrations"));
        });

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddSignInManager()
            .AddEntityFrameworkStores<PlatformDbContext>();

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IIdGenerator, UuidV7IdGenerator>();
        services.AddSingleton<IBlobStore, FileSystemBlobStore>();
        services.AddScoped<IMessagePublisher, WolverineMessagePublisher>();
        services.AddScoped<IDashboardSummaryQuery, EfDashboardSummaryQuery>();
        services.AddScoped<IAiCommandPublisher, DurableAiCommandPublisher>();
        services.AddSingleton<RabbitMqAiCommandPublisher>();
        services.AddScoped<ProcessedAiResultStore>();
        services.AddHostedService<OutboxDispatcherHostedService>();
        services.AddScoped<IChangeCommitter, EfChangeCommitter>();
        services.AddScoped<ISourceRepository, SourceRepository>();
        services.AddScoped<ISourceSnapshotRepository, SourceSnapshotRepository>();
        services.AddScoped<IEvidenceRepository, EvidenceRepository>();
        services.AddScoped<ISnapshotExistsQuery, SnapshotExistsQuery>();
        services.AddScoped<PromoteResearchFindingsHandler>();
        services.AddScoped<IClaimRepository, ClaimRepository>();
        services.AddScoped<ISourcesQuery, SourcesQuery>();
        services.AddScoped<ISnapshotsQuery, SnapshotsQuery>();
        services.AddScoped<IClaimReviewQueueQuery, ClaimReviewQueueQuery>();
        services.AddScoped<ISnapshotByHashQuery, SnapshotByHashQuery>();
        services.AddScoped<IResearchJobRepository, ResearchJobRepository>();
        services.AddScoped<IResearchJobsQuery, ResearchJobsQuery>();
        services.AddScoped<IContentUnitRepository, ContentUnitRepository>();
        services.AddScoped<IContentVersionRepository, ContentVersionRepository>();
        services.AddScoped<IContentUnitsQuery, ContentUnitsQuery>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IEditionRepository, EditionRepository>();
        services.AddScoped<IProductsQuery, ProductsQuery>();
        services.AddScoped<IEditionDetailQuery, EditionDetailQuery>();
        services.AddScoped<IPublicationPackageRepository, PublicationPackageRepository>();
        services.AddScoped<IApprovedContentVersionLookup, ApprovedContentVersionLookup>();
        services.AddScoped<IOperationRepository, OperationRepository>();
        services.AddScoped<IWebhookInboxRepository, WebhookInboxRepository>();
        services.AddScoped<IPurchaseRepository, PurchaseRepository>();
        services.AddScoped<IReconciliationRunRepository, ReconciliationRunRepository>();
        services.AddScoped<IPurchasesQuery, PurchasesQuery>();
        services.AddScoped<IWebhookSignatureVerifier, HmacSha256WebhookSignatureVerifier>();
        services.AddScoped<ILearnerRepository, LearnerRepository>();
        services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
        services.AddScoped<ICapstoneRepository, CapstoneRepository>();
        services.AddScoped<IEvaluationRepository, EvaluationRepository>();
        services.AddScoped<IOutcomeRepository, OutcomeRepository>();
        services.AddScoped<IOutcomesSummaryQuery, OutcomesSummaryQuery>();
        services.AddScoped<ICommerceOrderReconciler>(serviceProvider =>
        {
            var commerce = serviceProvider.GetRequiredService<IOptions<CommerceOptions>>().Value;
            return commerce.UseStubProvider
                ? serviceProvider.GetRequiredService<StubCommerceOrderReconciler>()
                : serviceProvider.GetRequiredService<HttpKiwifyOrderReconciler>();
        });
        services.AddScoped<StubCommerceOrderReconciler>();
        services.AddSingleton<KiwifyAccessTokenSource>();
        services.AddScoped<HttpKiwifyOrderReconciler>();
        services.AddScoped<CreateSourceHandler>();
        services.AddScoped<GetSourcesHandler>();
        services.AddScoped<GetSourceHandler>();
        services.AddScoped<CreateSnapshotHandler>();
        services.AddScoped<GetSnapshotsHandler>();
        services.AddScoped<GetClaimHandler>();
        services.AddScoped<GetClaimReviewQueueHandler>();
        services.AddScoped<ApproveClaimHandler>();
        services.AddScoped<RejectClaimHandler>();
        services.AddScoped<CreateClaimForReviewHandler>();
        services.AddScoped<CreateResearchJobHandler>();
        services.AddScoped<GetResearchJobHandler>();
        services.AddScoped<ListResearchJobsHandler>();
        services.AddScoped<ApplyResearchResultHandler>();
        services.AddScoped<CreateContentUnitHandler>();
        services.AddScoped<ListContentUnitsHandler>();
        services.AddScoped<GetContentVersionHandler>();
        services.AddScoped<UpdateContentVersionHandler>();
        services.AddScoped<RequestContentReviewHandler>();
        services.AddScoped<ApproveContentVersionHandler>();
        services.AddScoped<RequestContentChangesHandler>();
        services.AddScoped<ApplyContentGenerateResultHandler>();
        services.AddScoped<ApplyContentReviewResultHandler>();
        services.AddScoped<ListProductsHandler>();
        services.AddScoped<GetEditionHandler>();
        services.AddScoped<ReplaceCurriculumHandler>();
        services.AddScoped<CreatePublicationPackageHandler>();
        services.AddScoped<GetPublicationPackageHandler>();
        services.AddScoped<ExportPublicationPackageHandler>();
        services.AddScoped<ConfirmPublicationHandler>();
        services.AddScoped<GetOperationHandler>();
        services.AddScoped<ReceiveKiwifyWebhookHandler>();
        services.AddScoped<RunReconciliationHandler>();
        services.AddScoped<ListPurchasesHandler>();
        services.AddScoped<GetOutcomesSummaryHandler>();
        services.AddScoped<SubmitCapstoneHandler>();
        services.AddScoped<EvaluateCapstoneHandler>();
        services.AddHostedService<ProductionConfigurationGuard>();
        services.AddHostedService<DevelopmentSeedHostedService>();
        services.AddHostedService<AiResultConsumerHostedService>();

        return services;
    }
}
