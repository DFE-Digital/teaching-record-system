using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;
using OneOf;
using Optional;
using TeachingRecordSystem;
using TeachingRecordSystem.Api;
using TeachingRecordSystem.Api.Endpoints;
using TeachingRecordSystem.Api.Infrastructure.ApplicationModel;
using TeachingRecordSystem.Api.Infrastructure.Filters;
using TeachingRecordSystem.Api.Infrastructure.Logging;
using TeachingRecordSystem.Api.Infrastructure.Mapping;
using TeachingRecordSystem.Api.Infrastructure.Middleware;
using TeachingRecordSystem.Api.Infrastructure.ModelBinding;
using TeachingRecordSystem.Api.Infrastructure.OpenApi;
using TeachingRecordSystem.Api.Infrastructure.RateLimiting;
using TeachingRecordSystem.Api.Infrastructure.Redis;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Api.Validation;
using TeachingRecordSystem.Core.Infrastructure;
using TeachingRecordSystem.Core.Infrastructure.Json;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;
using TeachingRecordSystem.Core.Services.NameSynonyms;
using TeachingRecordSystem.Core.Services.Notify;
using TeachingRecordSystem.Core.Services.PersonMatching;
using TeachingRecordSystem.Core.Services.TrnRequests;
using TeachingRecordSystem.Core.Services.TrsDataSync;
using TeachingRecordSystem.Core.Services.Webhooks;
using TeachingRecordSystem.WebCommon;
using TeachingRecordSystem.WebCommon.Infrastructure.Logging;

[assembly: ApiController]

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

builder.AddServiceDefaults(dataProtectionBlobName: "Api");

var services = builder.Services;
var env = builder.Environment;
var configuration = builder.Configuration;

builder.ConfigureLogging((config, services) =>
{
    config.Enrich.With(ActivatorUtilities.CreateInstance<AddUserIdLogEventEnricher>(services));
});

services.AddAuthentication(ApiKeyAuthenticationHandler.AuthenticationScheme)
    .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationHandler.AuthenticationScheme, _ => { })
    .AddJwtBearer(AuthenticationSchemeNames.IdAccessToken, options =>
    {
        options.Authority = configuration["GetAnIdentity:BaseAddress"];
        options.MapInboundClaims = false;
        options.TokenValidationParameters.ValidateAudience = false;
        options.TokenValidationParameters.RequireExpirationTime = false;
    })
    .AddJwtBearer(AuthenticationSchemeNames.AuthorizeAccessAccessToken, options =>
    {
        options.Authority = configuration.GetRequiredValue("AuthorizeAccessIssuer");
        options.MapInboundClaims = false;
        options.TokenValidationParameters.ValidateAudience = false;
    });

services.AddAuthorization(options =>
{
    options.AddPolicy(
        AuthorizationPolicies.ApiKey,
        policy => policy
            .AddAuthenticationSchemes(ApiKeyAuthenticationHandler.AuthenticationScheme)
            .RequireClaim(ClaimTypes.Name));

    options.AddPolicy(
        AuthorizationPolicies.IdentityUserWithTrn,
        policy => policy
            .AddAuthenticationSchemes(AuthenticationSchemeNames.IdAccessToken, AuthenticationSchemeNames.AuthorizeAccessAccessToken)
            .RequireAssertion(ctx =>
            {
                var scopes = (ctx.User.FindFirstValue("scope") ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return scopes.Contains("dqt:read") || scopes.Contains("teaching_record");
            })
            .RequireClaim("trn"));
});

services
    .AddMvc(options =>
    {
        options.AddHybridBodyModelBinderProvider();

        options.Filters.Add(new ServiceFilterAttribute<AddTrnToSentryScopeResourceFilter>() { Order = -1 });
        options.Filters.Add(new CrmServiceProtectionFaultExceptionFilter());
        options.Filters.Add(new DefaultErrorExceptionFilter(statusCode: StatusCodes.Status400BadRequest));
        options.Filters.Add(new ValidationExceptionFilter());

        options.Conventions.Add(new ApiVersionConvention(builder.Configuration));
        options.Conventions.Add(new AuthorizationPolicyConvention());
        options.Conventions.Add(new BackFillVersionedEndpointsConvention());
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressInferBindingSourcesForParameters = true;
    });

services.AddHttpContextAccessor();

services.AddFluentValidationAutoValidation(options => options.DisableDataAnnotationsValidation = true);

services.AddValidatorsFromAssemblyContaining(typeof(Program));

services.AddTransient<IValidatorInterceptor, PreferModelBindingErrorsValidationInterceptor>();

services.AddTransient<IApiDescriptionProvider, HybridBodyApiDescriptionProvider>();

services
    .AddControllers(options =>
    {
        options.OutputFormatters.RemoveType<StringOutputFormatter>();
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new OneOfJsonConverterFactory());

        options.JsonSerializerOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        {
            Modifiers =
            {
                Modifiers.OptionProperties
            }
        };
    });

services.Decorate<Microsoft.AspNetCore.Mvc.Infrastructure.ProblemDetailsFactory, CamelCaseErrorKeysProblemDetailsFactory>();

services.AddOpenApi(configuration);

services
    .AddAutoMapper(cfg =>
    {
        cfg.AddMaps(typeof(Program).Assembly);
        cfg.CreateMap(typeof(Option<>), typeof(Option<>)).ConvertUsing(typeof(OptionToOptionTypeConverter<,>));
        cfg.CreateMap(typeof(OneOf<,>), typeof(OneOf<,>)).ConvertUsing(typeof(OneOfToOneOfTypeConverter<,,,>));
    })
    .AddTransient(typeof(WrapWithOptionValueConverter<>))
    .AddTransient(typeof(WrapWithOptionValueConverter<,>));

services.Scan(scan =>
{
    scan.FromAssemblyOf<Program>()
        .AddClasses(filter => filter.InNamespaces("TeachingRecordSystem.Api.V3.Implementation.Operations").Where(type => type.Name.EndsWith("Handler")))
            .AsSelf()
            .WithTransientLifetime();

    scan.FromAssemblyOf<Program>()
        .AddClasses(filter => filter.AssignableTo(typeof(ITypeConverter<,>)))
            .AsSelf()
            .WithTransientLifetime();
});

services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());
services.AddSingleton<ICurrentUserProvider, ClaimsPrincipalCurrentUserProvider>();
services.AddMemoryCache();
services.AddSingleton<AddTrnToSentryScopeResourceFilter>();

builder.Services.AddOptions<EvidenceFilesOptions>()
    .Bind(builder.Configuration.GetSection("EvidenceFiles"))
    .ValidateDataAnnotations()
    .ValidateOnStart();
services
    .AddTransient<DownloadEvidenceFilesFromBlobStorageHttpHandler>()
    .AddHttpClient("EvidenceFiles", client =>
    {
        client.MaxResponseContentBufferSize = 5 * 1024 * 1024;  // 5MB
        client.Timeout = TimeSpan.FromSeconds(30);
    })
    .AddHttpMessageHandler<DownloadEvidenceFilesFromBlobStorageHttpHandler>();

builder
    .AddBlobStorage()
    .AddDistributedLocks()
    .AddIdentityApi()
    .AddNameSynonyms()
    .AddWebhookOptions()
    .AddTrsSyncHelper()
    .AddTrnRequestService()
    .AddEmail();

services
    .AddTrsBaseServices()
    .AddAccessYourTeachingQualificationsOptions(configuration, env)
    .AddFileService()
    .AddTransient<GetPersonHelper>()
    .AddPersonMatching();

// ReferenceDataCache startup task is relatively slow and adversely impacts app startup time.
if (env.IsProduction())
{
    services.AddStartupTask<ReferenceDataCache>();
}

services.AddRedis(env, configuration);
services.AddRateLimiting(env, configuration);

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseMiddleware<AssignRequestedVersionMiddleware>();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

if (env.IsProduction())
{
    // Apply rate limiting to authenticated endpoints
    // (i.e. everywhere except health check, status endpoints etc.)
    app.UseWhen(ctx => ctx.User.Identity?.IsAuthenticated == true, x => x.UseRateLimiter());
}

app.Use((ctx, next) =>
{
    ctx.Response.Headers.Append("X-Frame-Options", "deny");
    ctx.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    ctx.Response.Headers.Append("X-XSS-Protection", "0");

    return next();
});

app.MapWebhookJwks();

app.MapControllers();

if (env.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}

app.Run();

public partial class Program;

file static class AuthenticationSchemeNames
{
    public const string IdAccessToken = nameof(IdAccessToken);
    public const string AuthorizeAccessAccessToken = nameof(AuthorizeAccessAccessToken);
}
