#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Claims;
using AspNetCoreRateLimit;
using AspNetCoreRateLimit.Redis;
using Azure.Storage.Blobs;
using FluentValidation;
using FluentValidation.AspNetCore;
using Medallion.Threading;
using Medallion.Threading.Azure;
using Medallion.Threading.FileSystem;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Microsoft.OpenApi.Models;
using Microsoft.PowerPlatform.Dataverse.Client;
using Prometheus;
using QualifiedTeachersApi.DataStore.Crm;
using QualifiedTeachersApi.DataStore.Sql;
using QualifiedTeachersApi.Filters;
using QualifiedTeachersApi.Infrastructure.ApplicationInsights;
using QualifiedTeachersApi.Infrastructure.ApplicationModel;
using QualifiedTeachersApi.Infrastructure.Configuration;
using QualifiedTeachersApi.Infrastructure.Json;
using QualifiedTeachersApi.Infrastructure.Logging;
using QualifiedTeachersApi.Infrastructure.ModelBinding;
using QualifiedTeachersApi.Infrastructure.Security;
using QualifiedTeachersApi.Infrastructure.Swagger;
using QualifiedTeachersApi.Services;
using QualifiedTeachersApi.Services.Certificates;
using QualifiedTeachersApi.Services.CrmEntityChanges;
using QualifiedTeachersApi.Services.DqtReporting;
using QualifiedTeachersApi.Services.GetAnIdentityApi;
using QualifiedTeachersApi.Services.TrnGenerationApi;
using QualifiedTeachersApi.Validation;
using Sentry.AspNetCore;
using Sentry.Extensibility;
using Serilog;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace QualifiedTeachersApi;

public class Program
{
    public static void Main(string[] args)
    {
        TypeDescriptor.AddAttributes(typeof(DateOnly), new TypeConverterAttribute(typeof(DateOnlyTypeConverter)));

        var builder = WebApplication.CreateBuilder(args);

        builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

        var services = builder.Services;
        var env = builder.Environment;
        var configuration = builder.Configuration;

        var paasEnvironmentName = configuration["PaasEnvironment"];

        builder.Host.UseSerilog((ctx, config) => config.ReadFrom.Configuration(ctx.Configuration));

        builder.Services.AddApplicationInsightsTelemetry()
            .AddApplicationInsightsTelemetryProcessor<RedactedUrlTelemetryProcessor>();

        builder.Services.AddFeatureManagement();

        if (env.IsProduction())
        {
            builder.WebHost.UseSentry();
        }

        if (builder.Environment.IsProduction())
        {
            builder.Configuration
                .AddJsonEnvironmentVariable("AppConfig")
                .AddJsonEnvironmentVariable("VCAP_SERVICES", configurationKeyPrefix: "VCAP_SERVICES")
                .AddJsonEnvironmentVariable("VCAP_APPLICATION", configurationKeyPrefix: "VCAP_APPLICATION");
        }

        services.AddAuthentication(ApiKeyAuthenticationHandler.AuthenticationScheme)
            .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationHandler.AuthenticationScheme, _ => { })
            .AddJwtBearer(options =>
            {
                options.Authority = configuration["GetAnIdentity:BaseAddress"];
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
                        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                        .RequireAssertion(ctx =>
                        {
                            var scopes = (ctx.User.FindFirstValue("scope") ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            return scopes.Contains("dqt:read");
                        })
                        .RequireClaim("trn"));
        });

        services
            .AddMvc(options =>
            {
                options.AddHybridBodyModelBinderProvider();

                options.Filters.Add(new ProducesJsonOrProblemAttribute());
                options.Filters.Add(new CrmServiceProtectionFaultExceptionFilter());
                options.Filters.Add(new DefaultErrorExceptionFilter(statusCode: StatusCodes.Status400BadRequest));
                options.Filters.Add(new ValidationExceptionFilter());

                options.Conventions.Add(new ApiVersionConvention());
                options.Conventions.Add(new AuthorizationPolicyConvention());
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

        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Configure();
            });

        services.Decorate<Microsoft.AspNetCore.Mvc.Infrastructure.ProblemDetailsFactory, CamelCaseErrorKeysProblemDetailsFactory>();

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo() { Title = "DQT API", Version = "v1" });
            c.SwaggerDoc("v2", new OpenApiInfo() { Title = "DQT API", Version = "v2" });
            c.SwaggerDoc("v3", new OpenApiInfo() { Title = "DQT API", Version = "v3" });

            c.DocInclusionPredicate((docName, api) => docName.Equals(api.GroupName, StringComparison.OrdinalIgnoreCase));
            c.EnableAnnotations();
            c.ExampleFilters();
            c.OperationFilter<ResponseContentTypeOperationFilter>();
            c.OperationFilter<RateLimitOperationFilter>();

            c.CustomSchemaIds(type =>
            {
                // Generated CRM models for custom entities have a weird type name; fix that up here
                // e.g. for the 'dfeta_inductionState' type use 'InductionState' in the API spec

                if (type.Name.StartsWith("dfeta_"))
                {
                    var prefixTrimmedTypeName = type.Name.Substring("dfeta_".Length);
                    return prefixTrimmedTypeName[0..1].ToUpper() + prefixTrimmedTypeName[1..];
                }

                return type.Name;
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
            {
                In = ParameterLocation.Header,
                Name = "Authorization",
                Scheme = "Bearer",
                Type = SecuritySchemeType.Http
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                [
                    new OpenApiSecurityScheme()
                    {
                        Reference = new OpenApiReference()
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    }
                ] = new List<string>()
            });
        });

        services.Configure<SentryAspNetCoreOptions>(options =>
        {
            if (!string.IsNullOrEmpty(paasEnvironmentName))
            {
                options.Environment = paasEnvironmentName;
            }

            var gitSha = configuration["GitSha"];
            if (!string.IsNullOrEmpty(gitSha))
            {
                options.Release = gitSha;
            }
        });

        var pgConnectionString = configuration.GetConnectionString("DefaultConnection");

        var healthCheckBuilder = services.AddHealthChecks()
            .AddNpgSql(pgConnectionString);

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());
        services.AddSingleton<IApiClientRepository, ConfigurationApiClientRepository>();
        services.AddSingleton<ICurrentClientProvider, ClaimsPrincipalCurrentClientProvider>();
        services.AddSwaggerExamplesFromAssemblyOf<Program>();
        services.AddTransient<ISerializerDataContractResolver>(sp =>
        {
            var serializerOptions = sp.GetRequiredService<IOptions<JsonOptions>>().Value.JsonSerializerOptions;
            return new Infrastructure.Swagger.JsonSerializerDataContractResolver(serializerOptions);
        });
        services.AddSingleton<ISchemaGenerator, Infrastructure.Swagger.SchemaGenerator>();
        services.AddSingleton<IClock, Clock>();
        services.AddMemoryCache();
        services.AddSingleton<ISentryEventProcessor, RemoveRedactedUrlParametersEventProcessor>();

        services.AddHttpClient("EvidenceFiles", client =>
        {
            client.MaxResponseContentBufferSize = 5 * 1024 * 1024;  // 5MB
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddDbContext<DqtContext>(
            options => DqtContext.ConfigureOptions(options, pgConnectionString),
            contextLifetime: ServiceLifetime.Transient,
            optionsLifetime: ServiceLifetime.Singleton);

        services.AddDbContextFactory<DqtContext>(options => DqtContext.ConfigureOptions(options, pgConnectionString));

        services.AddDatabaseDeveloperPageExceptionFilter();

        services.AddAzureClients(clientBuilder =>
        {
            clientBuilder.AddBlobServiceClient(configuration["StorageConnectionString"]);
        });

        if (env.IsProduction())
        {
            var containerName = configuration["DistributedLockContainerName"] ??
                throw new Exception("DistributedLockContainerName configuration key is missing.");

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

        services.AddTrnGenerationApi(configuration);
        services.AddIdentityApi(configuration, env);
        services.AddCertificateGeneration();
        services.AddCrmEntityChanges();
        services.AddDqtReporting(builder.Configuration);

        if (env.EnvironmentName != "Testing")
        {
            var crmServiceClient = GetCrmServiceClient();

            services.AddTransient<IOrganizationServiceAsync>(_ => crmServiceClient.Clone());
            services.AddTransient<IDataverseAdapter, DataverseAdapter>();

            healthCheckBuilder.AddCheck("CRM", () => crmServiceClient.IsReady ? HealthCheckResult.Healthy() : HealthCheckResult.Degraded());
        }

        if (env.IsProduction())
        {
            ConfigureRateLimitServices();
            ConfigureRedisServices();
        }

        MetricLabels.ConfigureLabels(builder.Configuration);

        var app = builder.Build();

        app.UseRequestLogging(logRequestBody: paasEnvironmentName != "prod");

        app.UseRouting();

        app.UseHttpMetrics();

        app.UseHealthChecks("/status");

        app.UseAuthentication();
        app.UseAuthorization();

        if (env.IsProduction())
        {
            app.UseMiddleware<RateLimitMiddleware>();
        }

        app.Use((ctx, next) =>
        {
            ctx.Response.Headers.Add("X-Frame-Options", "deny");
            ctx.Response.Headers.Add("X-Content-Type-Options", "nosniff");
            ctx.Response.Headers.Add("X-XSS-Protection", "0");

            return next();
        });

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGet("/health", async context =>
            {
                await context.Response.WriteAsync("OK");
            });

            endpoints.MapWebHookEndpoints();

            endpoints.MapMetrics();

            endpoints.MapControllers();
        });

        app.UseSwagger(options =>
        {
            options.PreSerializeFilters.Add((_, request) =>
            {
                request.HttpContext.Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
            });
        });

        if (env.IsDevelopment())
        {
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("v1/swagger.json", "DQT API v1");
                c.SwaggerEndpoint("v2/swagger.json", "DQT API v2");
                c.SwaggerEndpoint("v3/swagger.json", "DQT API v3");
                c.EnablePersistAuthorization();
            });

            app.UseMigrationsEndPoint();

            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DqtContext>();
                context.Database.EnsureCreated();
            }
        }

        if (env.IsProduction())
        {
            using (var scope = app.Services.CreateScope())
            {
                var clientPolicyStore = scope.ServiceProvider.GetRequiredService<IClientPolicyStore>();
                clientPolicyStore.SeedAsync().GetAwaiter().GetResult();
            }
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

            var connectionString = configuration.GetConnectionString("Crm") ?? throw new Exception("Crm connection string is missing.");

            return new ServiceClient(connectionString)
            {
                DisableCrossThreadSafeties = true,
                EnableAffinityCookie = true,
                MaxRetryCount = 2,
                RetryPauseTime = TimeSpan.FromSeconds(1)
            };
        }

        void ConfigureRateLimitServices()
        {
            services.Configure<ClientRateLimitOptions>(configuration.GetSection("ClientRateLimiting"));
            services.Configure<ClientRateLimitPolicies>(configuration.GetSection("ClientRateLimitPolicies"));

            services.AddDistributedRateLimiting<AsyncKeyLockProcessingStrategy>();
            services.AddDistributedRateLimiting<RedisProcessingStrategy>();
            services.AddRedisRateLimiting();

            services.AddSingleton<IClientPolicyStore, DistributedCacheClientPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, DistributedCacheRateLimitCounterStore>();
            services.AddSingleton<IRateLimitConfiguration, Infrastructure.Security.RateLimitConfiguration>();
        }

        void ConfigureRedisServices()
        {
            var connectionString = configuration.GetConnectionString("Redis");

            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(connectionString));
            services.AddStackExchangeRedisCache(options => options.Configuration = connectionString);

            healthCheckBuilder.AddRedis(connectionString);
        }
    }
}
