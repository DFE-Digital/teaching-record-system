using System.Security.Claims;
using Azure.Storage.Blobs;
using FastEndpoints;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using idunno.Authentication.Basic;
using Medallion.Threading;
using Medallion.Threading.Azure;
using Medallion.Threading.FileSystem;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.PowerPlatform.Dataverse.Client;
using Prometheus;
using QualifiedTeachersApi.DataStore.Crm;
using QualifiedTeachersApi.DataStore.Sql;
using QualifiedTeachersApi.Filters;
using QualifiedTeachersApi.Infrastructure.ApplicationModel;
using QualifiedTeachersApi.Infrastructure.Configuration;
using QualifiedTeachersApi.Infrastructure.FastEndpoints;
using QualifiedTeachersApi.Infrastructure.Json;
using QualifiedTeachersApi.Infrastructure.Logging;
using QualifiedTeachersApi.Infrastructure.ModelBinding;
using QualifiedTeachersApi.Infrastructure.OpenApi;
using QualifiedTeachersApi.Infrastructure.RateLimiting;
using QualifiedTeachersApi.Infrastructure.Redis;
using QualifiedTeachersApi.Infrastructure.Security;
using QualifiedTeachersApi.Jobs;
using QualifiedTeachersApi.Services;
using QualifiedTeachersApi.Services.AccessYourQualifications;
using QualifiedTeachersApi.Services.Certificates;
using QualifiedTeachersApi.Services.CrmEntityChanges;
using QualifiedTeachersApi.Services.DqtReporting;
using QualifiedTeachersApi.Services.GetAnIdentityApi;
using QualifiedTeachersApi.Services.Notify;
using QualifiedTeachersApi.Services.TrnGenerationApi;
using QualifiedTeachersApi.Validation;
using Serilog;

namespace QualifiedTeachersApi;

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

        var platform = configuration["Platform"] ?? throw new Exception("Missing 'Platform' configuration entry.");
        var platformEnvironmentName = configuration["PlatformEnvironment"];

        builder.ConfigureLogging(platformEnvironmentName);

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
                options.Realm = "QualifiedTeachersApi";
                options.Events = new BasicAuthenticationEvents
                {
                    OnValidateCredentials = context =>
                    {
                        var config = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
                        var username = config["AdminCredentials:Username"];
                        var password = config["AdminCredentials:Password"];

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

        var pgConnectionString = configuration.GetConnectionString("DefaultConnection") ??
            throw new Exception("Missing DefaultConnection connection string.");

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
        services.AddAccessYourQualifications(configuration, env);
        services.AddCertificateGeneration();
        services.AddCrmEntityChanges();
        services.AddDqtReporting(builder.Configuration);
        services.AddBackgroundJobs(env, configuration, pgConnectionString);
        services.AddEmail(env, configuration);

        if (env.EnvironmentName != "Testing")
        {
            var crmServiceClient = GetCrmServiceClient();

            services.AddTransient<IOrganizationServiceAsync>(_ => crmServiceClient.Clone());
            services.AddTransient<IDataverseAdapter, DataverseAdapter>();

            healthCheckBuilder.AddCheck("CRM", () => crmServiceClient.IsReady ? HealthCheckResult.Healthy() : HealthCheckResult.Degraded());
        }

        if (env.IsProduction())
        {
            services.AddRedis(env, configuration, healthCheckBuilder);
            services.AddRateLimiting(env, configuration);
        }

        if (platform == "PAAS")
        {
            MetricLabels.ConfigureLabels(builder.Configuration);
        }

        var app = builder.Build();

        // If we've been invoked with `config` as an argument, return the corresponding config key and exit
        if (args.Length == 2 && args[0] == "config")
        {
            var configKey = args[1];
            Console.WriteLine(app.Configuration[configKey]);
            return;
        }

        app.UseSerilogRequestLogging();

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

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGet("/health", async context =>
            {
                await context.Response.WriteAsync("OK");
            });

            endpoints.MapWebHookEndpoints();

            if (platform == "PAAS")
            {
                endpoints.MapMetrics();
            }

            endpoints.MapControllers();

            if (configuration.GetValue<bool>("RecurringJobs:Enabled") && !builder.Environment.IsUnitTests() && !builder.Environment.IsEndToEndTests())
            {
                endpoints.MapHangfireDashboardWithAuthorizationPolicy(AuthorizationPolicies.Hangfire, "/_hangfire");
            }
        });

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

            var connectionString = configuration.GetConnectionString("Crm") ?? throw new Exception("Crm connection string is missing.");

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
