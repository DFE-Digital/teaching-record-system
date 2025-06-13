using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.PowerPlatform.Dataverse.Client;
using OneOf;
using Optional;
using TeachingRecordSystem.Api.Endpoints;
using TeachingRecordSystem.Api.Endpoints.IdentityWebHooks;
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
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Infrastructure;
using TeachingRecordSystem.Core.Infrastructure.Json;
using TeachingRecordSystem.Core.Services.DqtOutbox;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;
using TeachingRecordSystem.Core.Services.NameSynonyms;
using TeachingRecordSystem.Core.Services.TrnGeneration;
using TeachingRecordSystem.Core.Services.TrsDataSync;
using TeachingRecordSystem.Core.Services.Webhooks;
using TeachingRecordSystem.WebCommon;
using TeachingRecordSystem.WebCommon.Infrastructure.Logging;

[assembly: ApiController]
namespace TeachingRecordSystem.Api;

public class Program
{
    public static void Main(string[] args)
    {
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
            .AddDqtOutboxMessageSerializer()
            .AddWebhookOptions()
            .AddTrsSyncHelper();

        services.AddAccessYourTeachingQualificationsOptions(configuration, env);
        services.AddTrsBaseServices();
        services.AddFileService();
        services.AddTransient<GetPersonHelper>();

        if (!env.IsUnitTests())
        {
            var crmServiceClient = GetCrmServiceClient();
            services.AddApiTrnGeneration(configuration);
            services.AddPooledDefaultServiceClient(crmServiceClient, size: 200);
            services.AddTransient<IDataverseAdapter, DataverseAdapter>();

            services.AddHealthChecks()
                .AddCheck("CRM", () => crmServiceClient.IsReady ? HealthCheckResult.Healthy() : HealthCheckResult.Degraded());

            if (!env.IsDevelopment())
            {
                services.AddStartupTask<ReferenceDataCache>();
            }
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

        app.MapWebHookEndpoints();
        app.MapWebhookJwks();

        app.MapControllers();

        if (env.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }

        app.Run();

        ServiceClient GetCrmServiceClient()
        {
            // This property is poorly-named. It's really a request timeout.
            // It's worth noting this is a client-side timeout; it's not respected by the server.
            // If this timeout fires the operation is still going to complete on the server.
            //
            // It's important for some of our operations that we never see this timeout fire;
            // we have advisory locks in place that surround these operations that are dropped once this timeout
            // fires, even though the operation in CRM is still going on.
            ServiceClient.MaxConnectionTimeout = TimeSpan.FromMinutes(5);

            var connectionString = configuration.GetRequiredValue("ConnectionStrings:Crm");

            return new ServiceClient(connectionString)
            {
                DisableCrossThreadSafeties = true,
                EnableAffinityCookie = true,
                MaxRetryCount = 2,
                RetryPauseTime = TimeSpan.FromSeconds(1)
            };
        }
    }
}

file static class AuthenticationSchemeNames
{
    public const string IdAccessToken = nameof(IdAccessToken);
    public const string AuthorizeAccessAccessToken = nameof(AuthorizeAccessAccessToken);
}
