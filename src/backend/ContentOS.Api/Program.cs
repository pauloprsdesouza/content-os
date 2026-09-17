using ContentOS.Api.Http;
using ContentOS.Api.Knowledge;
using ContentOS.Contracts.Auth;
using ContentOS.Contracts.Dashboard;
using ContentOS.Domain.Identity;
using ContentOS.Infrastructure;
using ContentOS.Infrastructure.Identity;
using ContentOS.Infrastructure.Options;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Serilog;
using Wolverine;
using Wolverine.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console());
builder.Host.UseWolverine();

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddWolverineHttp();
builder.Services.AddHealthChecks();
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.HeaderName = "X-CSRF-TOKEN";
});
builder.Services.AddContentOSInfrastructure(builder.Configuration);
builder.Services
    .AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        CapabilityNames.KnowledgeApprove,
        policy => policy
            .RequireAuthenticatedUser()
            .RequireClaim("capability", CapabilityNames.KnowledgeApprove));
});
builder.Services
    .AddOptions<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme)
    .Configure<IOptions<SecurityOptions>, IHostEnvironment>((cookie, securityOptions, environment) =>
    {
        cookie.Cookie.Name = securityOptions.Value.CookieName;
        cookie.Cookie.HttpOnly = true;
        cookie.Cookie.SameSite = SameSiteMode.Strict;
        cookie.Cookie.SecurePolicy = environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        cookie.ExpireTimeSpan = securityOptions.Value.SessionLifetime;
        cookie.SlidingExpiration = true;
        cookie.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        cookie.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });

var app = builder.Build();

app.UseExceptionHandler();
app.UseSerilogRequestLogging();
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapOpenApi();
app.MapWolverineEndpoints();
app.MapHealthChecks("/health");
app.MapKnowledgeEndpoints();

var api = app.MapGroup("/api/v1");
var auth = api.MapGroup("/auth");

auth.MapGet("/antiforgery", (IAntiforgery antiforgery, HttpContext context) =>
{
    var tokens = antiforgery.GetAndStoreTokens(context);
    return Results.Ok(new AntiforgeryTokenResponse(tokens.RequestToken!));
});

auth.MapGet("/session", (HttpContext context) =>
    Results.Ok(new SessionResponse(
        context.User.Identity?.IsAuthenticated == true,
        context.User.Identity?.Name)));

auth.MapPost(
        "/login",
        async (
            LoginRequest request,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager) =>
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Invalid credentials");
            }

            var result = await signInManager.PasswordSignInAsync(
                user,
                request.Password,
                request.RememberMe,
                lockoutOnFailure: true);

            return result.Succeeded
                ? Results.NoContent()
                : Results.Problem(
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Invalid credentials");
        })
    .WithMetadata(RequireRequestAntiforgery.Instance);

auth.MapPost(
        "/logout",
        async (SignInManager<ApplicationUser> signInManager) =>
        {
            await signInManager.SignOutAsync();
            return Results.NoContent();
        })
    .RequireAuthorization()
    .WithMetadata(RequireRequestAntiforgery.Instance);

api.MapGet(
    "/dashboard/summary",
    () => Results.Ok(new DashboardSummaryResponse(0, 0, 0, 0)));

app.Run();
