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
using Microsoft.Identity.Web;
using TeachingRecordSystem;
using TeachingRecordSystem.Core;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Infrastructure.Configuration;
using TeachingRecordSystem.SupportUi;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddDistributedMemoryCache();

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
            var subject = ctx.Principal!.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = ctx.Principal!.FindFirstValue(ClaimTypes.Email);

            using var dbContext = ctx.HttpContext.RequestServices.GetRequiredService<TrsDbContext>();

            var user = await dbContext.Users.SingleOrDefaultAsync(u => u.AzureAdSubject == subject);

            if (user is null)
            {
                // We couldn't find a user by principal, but we may find them via email
                // (the CLI commmand to add a user creates a record *without* the AD subject).

                user = await dbContext.Users.SingleOrDefaultAsync(u => u.Email == email && u.Active == true && u.AzureAdSubject == null);

                if (user is not null)
                {
                    user.AzureAdSubject = subject;
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

builder.Services.AddRazorPages().AddMvcOptions(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});

builder.Services.AddTransient<TrsLinkGenerator>();

builder.Services.AddDbContext<TrsDbContext>(
    options => TrsDbContext.ConfigureOptions(options, pgConnectionString),
    contextLifetime: ServiceLifetime.Transient,
    optionsLifetime: ServiceLifetime.Singleton);

builder.Services.AddDbContextFactory<TrsDbContext>(options => TrsDbContext.ConfigureOptions(options, pgConnectionString));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseForwardedHeaders();
    app.UseHsts();
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
