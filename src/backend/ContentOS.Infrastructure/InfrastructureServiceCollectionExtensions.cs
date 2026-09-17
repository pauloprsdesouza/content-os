using ContentOS.Application.Blobs;
using ContentOS.Application.Messaging;
using ContentOS.Infrastructure.Blobs;
using ContentOS.Infrastructure.Ids;
using ContentOS.Infrastructure.Identity;
using ContentOS.Infrastructure.Messaging;
using ContentOS.Infrastructure.Options;
using ContentOS.Infrastructure.Persistence;
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
        services.AddOptions<SecurityOptions>()
            .Bind(configuration.GetSection(SecurityOptions.SectionName));

        services.AddDbContext<PlatformDbContext>((serviceProvider, options) =>
        {
            var connectionString = serviceProvider
                .GetRequiredService<IOptions<ConnectionStringsOptions>>()
                .Value
                .Platform;
            options.UseNpgsql(connectionString);
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

        return services;
    }
}
