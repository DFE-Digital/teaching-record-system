using System.Security.Claims;
using GovUk.Frontend.AspNetCore;
using Joonasw.AspNetCore.SecurityHeaders;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Identity.Web;
using TeachingRecordSystem;
using TeachingRecordSystem.Core;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Infrastructure.Configuration;
using TeachingRecordSystem.SupportUi;
using TeachingRecordSystem.SupportUi.Infrastructure;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;
using TeachingRecordSystem.SupportUi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

if (builder.Environment.IsProduction())
{
    builder.Configuration.AddJsonEnvironmentVariable("AppConfig");
}

var pgConnectionString = builder.Configuration.GetRequiredValue("ConnectionStrings:DefaultConnection");

if (builder.Environment.IsProduction())
{
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.All;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });

    builder.Services.AddHsts(options =>
    {
        options.Preload = true;
        options.IncludeSubDomains = true;
        options.MaxAge = TimeSpan.FromDays(365);
    });

    builder.Services.AddDataProtection()
        .PersistKeysToAzureBlobStorage(
            builder.Configuration.GetRequiredValue("StorageConnectionString"),
            builder.Configuration.GetRequiredValue("DataProtectionKeysContainerName"),
            "SupportUi");
}

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton<IDistributedCache, DevelopmentFileDistributedCache>();
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

builder.Services.AddGovUkFrontend();
builder.Services.AddCsp(nonceByteAmount: 32);

if (!builder.Environment.IsUnitTests() && !builder.Environment.IsEndToEndTests())
{
    var graphApiScopes = new[] { "User.Read", "User.ReadBasic.All" };

    builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(builder.Configuration, "AzureAd")
        .EnableTokenAcquisitionToCallDownstreamApi(initialScopes: graphApiScopes)
        .AddDistributedTokenCaches()
        .AddMicrosoftGraph(defaultScopes: graphApiScopes);

    builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        options.Scope.Add("email");

        options.Events.OnTicketReceived = async ctx =>
        {
            var userId = ctx.Principal!.FindFirstValue("uid") ?? throw new Exception("Missing uid claim.");
            var email = ctx.Principal!.FindFirstValue(ClaimTypes.Email) ?? throw new Exception("Missing email address claim.");

            using var dbContext = ctx.HttpContext.RequestServices.GetRequiredService<TrsDbContext>();

            var user = await dbContext.Users.SingleOrDefaultAsync(u => u.AzureAdUserId == userId);

            if (user is null)
            {
                // We couldn't find a user by principal, but we may find them via email
                // (the CLI commmand to add a user creates a record *without* the AD subject).

                user = await dbContext.Users.SingleOrDefaultAsync(u => u.Email == email && u.Active == true && u.AzureAdUserId == null);

                if (user is not null)
                {
                    user.AzureAdUserId = userId;
                    await dbContext.SaveChangesAsync();
                }
            }

            if (user is not null)
            {
                var identityWithRoles = new ClaimsIdentity(
                    ctx.Principal!.Identity,
                    user.Roles.Select(r => new Claim(ClaimTypes.Role, r))
                        .Append(new Claim(CustomClaims.UserId, user.UserId.ToString())));

                ctx.Principal = new ClaimsPrincipal(identityWithRoles);
            }
        };
    });

    builder.Services.Configure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.Name = "trs-auth";

        options.Events.OnSigningOut = ctx =>
        {
            ctx.Response.Redirect("/signed-out");
            return Task.CompletedTask;
        };
    });
}

builder.Services
    .AddRazorPages()
    .AddMvcOptions(options =>
    {
        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
        options.Filters.Add(new AuthorizeFilter(policy));

        options.Filters.Add(new CheckUserExistsFilter());
    })
    .AddCookieTempDataProvider(options =>
    {
        options.Cookie.Name = "trs-tempdata";
    });

var healthCheckBuilder = builder.Services.AddHealthChecks()
    .AddNpgSql(pgConnectionString);

builder.Services.AddDbContext<TrsDbContext>(
    options => TrsDbContext.ConfigureOptions(options, pgConnectionString),
    contextLifetime: ServiceLifetime.Transient,
    optionsLifetime: ServiceLifetime.Singleton);

builder.Services.AddDbContextFactory<TrsDbContext>(options => TrsDbContext.ConfigureOptions(options, pgConnectionString));

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}

builder.Services
    .AddTransient<TrsLinkGenerator>()
    .AddTransient<CheckUserExistsFilter>()
    .AddSingleton<IClock, Clock>()
    .AddSupportUiServices(builder.Configuration, builder.Environment);

var app = builder.Build();

if (app.Environment.IsProduction())
{
    app.UseForwardedHeaders();
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else if (!app.Environment.IsUnitTests())
{
    app.UseExceptionHandler("/error");
    app.UseStatusCodePagesWithReExecute("/error", "?code={0}");
}

app.UseCsp(csp =>
{
    var pageTemplateHelper = app.Services.GetRequiredService<PageTemplateHelper>();

    csp.ByDefaultAllow
        .FromSelf();

    csp.AllowScripts
        .FromSelf()
        .From(pageTemplateHelper.GetCspScriptHashes())
        .AddNonce();

    // Ensure ASP.NET Core's auto refresh works
    // See https://github.com/dotnet/aspnetcore/issues/33068
    if (builder.Environment.IsDevelopment())
    {
        csp.AllowConnections
            .ToAnywhere();
    }
});

app.UseHealthChecks("/status");

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", async context =>
{
    await context.Response.WriteAsync("OK");
});

app.MapRazorPages();
app.MapControllers();

app.Run();

public partial class Program { }
