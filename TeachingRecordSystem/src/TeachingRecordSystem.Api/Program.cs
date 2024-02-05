using System.Security.Claims;
using FluentValidation;
using FluentValidation.AspNetCore;
using idunno.Authentication.Basic;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Api.Endpoints.IdentityWebHooks;
using TeachingRecordSystem.Api.Infrastructure.ApplicationModel;
using TeachingRecordSystem.Api.Infrastructure.Filters;
using TeachingRecordSystem.Api.Infrastructure.Json;
using TeachingRecordSystem.Api.Infrastructure.Logging;
using TeachingRecordSystem.Api.Infrastructure.ModelBinding;
using TeachingRecordSystem.Api.Infrastructure.OpenApi;
using TeachingRecordSystem.Api.Infrastructure.RateLimiting;
using TeachingRecordSystem.Api.Infrastructure.Redis;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.Validation;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Infrastructure;
using TeachingRecordSystem.Core.Services.Certificates;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;
using TeachingRecordSystem.Core.Services.TrnGenerationApi;
using TeachingRecordSystem.ServiceDefaults;

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

        builder.ConfigureLogging();

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
                AuthorizationPolicies.GetPerson,
                policy => policy
                    .AddAuthenticationSchemes(ApiKeyAuthenticationHandler.AuthenticationScheme)
                    .RequireRole([ApiRoles.GetPerson, ApiRoles.UpdatePerson]));

            options.AddPolicy(
                AuthorizationPolicies.UpdatePerson,
                policy => policy
                    .AddAuthenticationSchemes(ApiKeyAuthenticationHandler.AuthenticationScheme)
                    .RequireRole([ApiRoles.UpdatePerson]));

            options.AddPolicy(
                AuthorizationPolicies.UpdateNpq,
                policy => policy
                    .AddAuthenticationSchemes(ApiKeyAuthenticationHandler.AuthenticationScheme)
                    .RequireRole([ApiRoles.UpdateNpq]));

            options.AddPolicy(
                AuthorizationPolicies.UnlockPerson,
                policy => policy
                    .AddAuthenticationSchemes(ApiKeyAuthenticationHandler.AuthenticationScheme)
                    .RequireRole([ApiRoles.UnlockPerson]));

            options.AddPolicy(
              AuthorizationPolicies.CreateTrn,
              policy => policy
                  .AddAuthenticationSchemes(ApiKeyAuthenticationHandler.AuthenticationScheme)
                  .RequireRole([ApiRoles.CreateTrn]));
        });

        services
            .AddMvc(options =>
            {
                options.AddHybridBodyModelBinderProvider();

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
                options.JsonSerializerOptions.Configure();
            });

        services.Decorate<Microsoft.AspNetCore.Mvc.Infrastructure.ProblemDetailsFactory, CamelCaseErrorKeysProblemDetailsFactory>();

        services.AddOpenApi(configuration);

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());
        services.AddSingleton<ICurrentClientProvider, ClaimsPrincipalCurrentClientProvider>();
        services.AddSingleton<IClock, Clock>();
        services.AddMemoryCache();

        services.AddHttpClient("EvidenceFiles", client =>
        {
            client.MaxResponseContentBufferSize = 5 * 1024 * 1024;  // 5MB
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        builder
            .AddBlobStorage()
            .AddDistributedLocks()
            .AddIdentityApi();

        services.AddAccessYourTeachingQualificationsOptions(configuration, env);
        services.AddCertificateGeneration();
        services.AddCrmQueries();

        if (!env.IsUnitTests())
        {
            var crmServiceClient = GetCrmServiceClient();
            services.AddTrnGenerationApi(configuration);
            services.AddDefaultServiceClient(ServiceLifetime.Transient, _ => crmServiceClient.Clone());
            services.AddTransient<IDataverseAdapter, DataverseAdapter>();

            services.AddHealthChecks()
                .AddCheck("CRM", () => crmServiceClient.IsReady ? HealthCheckResult.Healthy() : HealthCheckResult.Degraded());
        }

        services.AddRedis(env, configuration);
        services.AddRateLimiting(env, configuration);

        var app = builder.Build();

        app.MapDefaultEndpoints();

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
