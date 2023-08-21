using System.Security.Claims;
using Azure.Storage.Blobs;
using FastEndpoints;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.PostgreSql;
using idunno.Authentication.Basic;
using Medallion.Threading;
using Medallion.Threading.Azure;
using Medallion.Threading.FileSystem;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.PowerPlatform.Dataverse.Client;
using Prometheus;
using TeachingRecordSystem.Api.Endpoints.IdentityWebHooks;
using TeachingRecordSystem.Api.Infrastructure.ApplicationModel;
using TeachingRecordSystem.Api.Infrastructure.Configuration;
using TeachingRecordSystem.Api.Infrastructure.FastEndpoints;
using TeachingRecordSystem.Api.Infrastructure.Filters;
using TeachingRecordSystem.Api.Infrastructure.Json;
using TeachingRecordSystem.Api.Infrastructure.Logging;
using TeachingRecordSystem.Api.Infrastructure.ModelBinding;
using TeachingRecordSystem.Api.Infrastructure.OpenApi;
using TeachingRecordSystem.Api.Infrastructure.RateLimiting;
using TeachingRecordSystem.Api.Infrastructure.Redis;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.Validation;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Services.CrmEntityChanges;
using TeachingRecordSystem.Core.Dqt.Services.DqtReporting;
using TeachingRecordSystem.Core.Infrastructure.Configuration;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Services.AccessYourQualifications;
using TeachingRecordSystem.Core.Services.Certificates;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;
using TeachingRecordSystem.Core.Services.Notify;
using TeachingRecordSystem.Core.Services.TrnGenerationApi;

namespace TeachingRecordSystem.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

        var services = builder.Services;
        var env = builder.Environment;
        var configuration = builder.Configuration;

        if (builder.Environment.IsProduction())
        {
            builder.Configuration
                .AddJsonEnvironmentVariable("AppConfig")
                .AddJsonEnvironmentVariable("VCAP_APPLICATION", configurationKeyPrefix: "VCAP_APPLICATION");
        }

        var platformEnvironmentName = configuration["PlatformEnvironment"];
        builder.ConfigureLogging(platformEnvironmentName);

        var platform = configuration.GetRequiredValue("Platform");
        var pgConnectionString = configuration.GetRequiredValue("ConnectionStrings:DefaultConnection");

        services.AddAuthentication(ApiKeyAuthenticationHandler.AuthenticationScheme)
            .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationHandler.AuthenticationScheme, _ => { })
            .AddJwtBearer(options =>
            {
                options.Authority = configuration["GetAnIdentity:BaseAddress"];
                options.MapInboundClaims = false;
                options.TokenValidationParameters.ValidateAudience = false;
            })
            .AddBasic(options =>
            {
                options.Realm = "TeachingRecordSystem.Api";
                options.Events = new BasicAuthenticationEvents
                {
                    OnValidateCredentials = static context =>
                    {
                        var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
                        var username = configuration.GetRequiredValue("AdminCredentials:Username");
                        var password = configuration.GetRequiredValue("AdminCredentials:Password");

                        if (context.Username == username && context.Password == password)
                        {
                            var claims = new[]
                            {
                                new Claim(
                                    ClaimTypes.NameIdentifier,
                                    context.Username,
                                    ClaimValueTypes.String,
                                    context.Options.ClaimsIssuer),
                                new Claim(
                                    ClaimTypes.Name,
                                    context.Username,
                                    ClaimValueTypes.String,
                                    context.Options.ClaimsIssuer)
                            };

                            context.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, context.Scheme.Name));
                            context.Success();
                        }

                        return Task.CompletedTask;
                    }
                };
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
                        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                        .RequireAssertion(ctx =>
                        {
                            var scopes = (ctx.User.FindFirstValue("scope") ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            return scopes.Contains("dqt:read");
                        })
                        .RequireClaim("trn"));

            options.AddPolicy(
                AuthorizationPolicies.Hangfire,
                policy => policy
                    .AddAuthenticationSchemes(BasicAuthenticationDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                );
        });

        services
            .AddMvc(options =>
            {
                options.AddHybridBodyModelBinderProvider();

                options.Filters.Add(new CrmServiceProtectionFaultExceptionFilter());
                options.Filters.Add(new DefaultErrorExceptionFilter(statusCode: StatusCodes.Status400BadRequest));
                options.Filters.Add(new ValidationExceptionFilter());
                options.Filters.Add(new ReadOnlyModeFilterFactory());

                options.Conventions.Add(new ApiVersionConvention());
                options.Conventions.Add(new AuthorizationPolicyConvention());
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressInferBindingSourcesForParameters = true;
            });

        services.AddFastEndpoints();

        services.AddHttpContextAccessor();

        services.AddFluentValidationAutoValidation(options => options.DisableDataAnnotationsValidation = true);

        services.AddValidatorsFromAssemblyContaining(typeof(Program));

        services.AddTransient<IValidatorInterceptor, PreferModelBindingErrorsValidationInterceptor>();

        services.AddTransient<IApiDescriptionProvider, HybridBodyApiDescriptionProvider>();

        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Configure();
            });

        services.Decorate<Microsoft.AspNetCore.Mvc.Infrastructure.ProblemDetailsFactory, CamelCaseErrorKeysProblemDetailsFactory>();

        services.AddOpenApi(configuration);

        var healthCheckBuilder = services.AddHealthChecks()
            .AddNpgSql(pgConnectionString);

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());
        services.AddSingleton<IApiClientRepository, ConfigurationApiClientRepository>();
        services.AddSingleton<ICurrentClientProvider, ClaimsPrincipalCurrentClientProvider>();
        services.AddSingleton<IClock, Clock>();
        services.AddMemoryCache();
        services.AddSingleton<ReadOnlyModeFilter>();

        services.AddHttpClient("EvidenceFiles", client =>
        {
            client.MaxResponseContentBufferSize = 5 * 1024 * 1024;  // 5MB
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddDbContext<TrsDbContext>(
            options => TrsDbContext.ConfigureOptions(options, pgConnectionString),
            contextLifetime: ServiceLifetime.Transient,
            optionsLifetime: ServiceLifetime.Singleton);

        services.AddDbContextFactory<TrsDbContext>(options => TrsDbContext.ConfigureOptions(options, pgConnectionString));

        services.AddDatabaseDeveloperPageExceptionFilter();

        if (!env.IsUnitTests())
        {
            services.AddAzureClients(clientBuilder =>
            {
                clientBuilder.AddBlobServiceClient(configuration.GetRequiredValue("StorageConnectionString"));
            });
        }

        if (env.IsProduction())
        {
            var containerName = configuration.GetRequiredValue("DistributedLockContainerName");

            services.AddSingleton<IDistributedLockProvider>(sp =>
            {
                var blobServiceClient = sp.GetRequiredService<BlobServiceClient>();
                var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
                return new AzureBlobLeaseDistributedSynchronizationProvider(blobContainerClient);
            });
        }
        else
        {
            var lockFileDirectory = Path.Combine(Path.GetTempPath(), "qtlocks");
            services.AddSingleton<IDistributedLockProvider>(new FileDistributedSynchronizationProvider(new DirectoryInfo(lockFileDirectory)));
        }

        if (env.IsProduction())
        {
            services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(pgConnectionString));

            services.AddHangfireServer();
        }

        services.AddTrnGenerationApi(configuration);
        services.AddIdentityApi(configuration, env);
        services.AddAccessYourQualifications(configuration, env);
        services.AddCertificateGeneration();
        services.AddCrmEntityChanges();
        services.AddDqtReporting(builder.Configuration);
        services.AddBackgroundJobs(env, configuration);
        services.AddEmail(env, configuration);
        services.AddCrmQueries();
        services.AddSingleton<ReferenceDataCache>();

        // Filter telemetry emitted by DqtReportingService
        services.AddApplicationInsightsTelemetry()
            .AddApplicationInsightsTelemetryProcessor<IgnoreDependencyTelemetryProcessor>();

        if (!env.IsUnitTests())
        {
            var crmServiceClient = GetCrmServiceClient();

            services.AddTransient<IOrganizationServiceAsync>(_ => crmServiceClient.Clone());
            services.AddTransient<IDataverseAdapter, DataverseAdapter>();

            healthCheckBuilder.AddCheck("CRM", () => crmServiceClient.IsReady ? HealthCheckResult.Healthy() : HealthCheckResult.Degraded());
        }

        services.AddRedis(env, configuration, healthCheckBuilder);
        services.AddRateLimiting(env, configuration);

        if (platform == "PAAS")
        {
            MetricLabels.ConfigureLabels(builder.Configuration);
        }

        if (builder.Environment.IsProduction())
        {
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.All;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            builder.Services.AddDataProtection()
                .PersistKeysToAzureBlobStorage(
                    configuration.GetRequiredValue("StorageConnectionString"),
                    configuration.GetRequiredValue("DataProtectionKeysContainerName"),
                    "Api");
        }

        var app = builder.Build();

        // If we've been invoked with `config` as an argument, return the corresponding config key and exit
        if (args.Length == 2 && args[0] == "config")
        {
            var configKey = args[1];
            Console.WriteLine(app.Configuration[configKey]);
            return;
        }

        if (app.Environment.IsProduction())
        {
            app.UseForwardedHeaders();
        }

        app.UseRouting();

        if (platform == "PAAS")
        {
            app.UseHttpMetrics();
        }

        app.UseHealthChecks("/status");

        app.UseAuthentication();
        app.UseAuthorization();

        if (env.IsProduction())
        {
            app.UseRateLimiter();
        }

        app.Use((ctx, next) =>
        {
            ctx.Response.Headers.Add("X-Frame-Options", "deny");
            ctx.Response.Headers.Add("X-Content-Type-Options", "nosniff");
            ctx.Response.Headers.Add("X-XSS-Protection", "0");

            return next();
        });

        app.UseFastEndpoints(c =>
        {
            c.Binding.ValueParserFor<DateOnly>(Parsers.DateOnlyParser);
            c.Binding.ValueParserFor<DateOnly?>(Parsers.NullableDateOnlyParser);
            c.Binding.FailureMessage = (propertyType, propertyName, attemptedValue) => $"'{attemptedValue}' is not valid.";
            c.Endpoints.Configurator = ep =>
            {
                ep.Description(x => x.ClearDefaultProduces(401));
            };
            c.Errors.ProducesMetadataType = typeof(HttpValidationProblemDetails);
            c.Errors.ResponseBuilder = (failures, ctx, statusCode) =>
            {
                var errors = failures
                    .GroupBy(f => c.Serializer.Options.PropertyNamingPolicy?.ConvertName(f.PropertyName) ?? f.PropertyName)
                    .ToDictionary(e => e.Key, e => e.Select(m => m.ErrorMessage).ToArray());

                return new ValidationProblemDetails(errors)
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    Title = "One or more validation errors occurred.",
                    Status = statusCode,
                    Instance = ctx.Request.Path,
                    Extensions =
                    {
                        { "traceId", ctx.TraceIdentifier }
                    }
                };
            };
            c.Serializer.Options.Configure();
            c.Versioning.Prefix = "v";
            c.Versioning.PrependToRoute = true;
        });

        app.MapGet("/health", async context =>
        {
            await context.Response.WriteAsync("OK");
        });

        app.MapWebHookEndpoints();

        if (platform == "PAAS")
        {
            app.MapMetrics();
        }

        app.MapControllers();

        if (configuration.GetValue<bool>("RecurringJobs:Enabled") && !builder.Environment.IsUnitTests() && !builder.Environment.IsEndToEndTests())
        {
            app.MapHangfireDashboardWithAuthorizationPolicy(AuthorizationPolicies.Hangfire, "/_hangfire");
        }

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
