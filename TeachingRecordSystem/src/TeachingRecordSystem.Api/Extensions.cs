using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using OneOf;
using Optional;
using TeachingRecordSystem.Api.Infrastructure.ApplicationModel;
using TeachingRecordSystem.Api.Infrastructure.Filters;
using TeachingRecordSystem.Api.Infrastructure.Mapping;
using TeachingRecordSystem.Api.Infrastructure.ModelBinding;
using TeachingRecordSystem.Api.Infrastructure.OpenApi;
using TeachingRecordSystem.Api.Infrastructure.RateLimiting;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Api.Validation;
using TeachingRecordSystem.Core.Infrastructure.Json;
using TeachingRecordSystem.Core.Services.GetAnIdentity;
using TeachingRecordSystem.Core.Services.Webhooks;
using TeachingRecordSystem.WebCommon.Filters;

namespace TeachingRecordSystem.Api;

public static class Extensions
{
    public static IServiceCollection AddApiCommands(this IServiceCollection services)
    {
        services.Scan(scan =>
            scan.FromAssemblyOf<Program>()
                .AddClasses(filter => filter.AssignableTo(typeof(ICommandHandler<,>)))
                .AsImplementedInterfaces()
                .WithTransientLifetime());

        services.AddTransient<ICommandDispatcher, CommandDispatcher>();

        return services;
    }

    public static IHostApplicationBuilder AddApiServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddApiServices(builder.Configuration, builder.Environment);

        return builder;
    }

    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.Scan(scan =>
        {
            scan.FromAssemblies(typeof(Extensions).Assembly)
                .AddClasses(filter => filter.AssignableTo(typeof(ITypeConverter<,>)))
                .AsSelf()
                .WithTransientLifetime();
        });

        services
            .AddMvc(options =>
            {
                options.AddHybridBodyModelBinderProvider();

                options.Filters.Add(new ServiceFilterAttribute<AddTrnToSentryScopeResourceFilter>() { Order = -1 });
                options.Filters.Add(new DefaultErrorExceptionFilter(statusCode: StatusCodes.Status400BadRequest));
                options.Filters.Add(new ValidationExceptionFilter());

                options.Conventions.Add(new ApiVersionConvention(configuration));
                options.Conventions.Add(new AuthorizationPolicyConvention());
                options.Conventions.Add(new BackFillVersionedEndpointsConvention());
                options.Filters.Add(new NoCachePageFilter());

                options.OutputFormatters.RemoveType<StringOutputFormatter>();
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressInferBindingSourcesForParameters = true;
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
            .AddApiCommands()
            .AddWebhookOptions(configuration)
            .AddOpenApi(configuration)
            .AddFluentValidation()
            .AddAutoMapper()
            .AddHttpContextAccessor()
            .AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>())
            .AddSingleton<ICurrentUserProvider, ClaimsPrincipalCurrentUserProvider>()
            .AddMemoryCache()
            .AddSingleton<AddTrnToSentryScopeResourceFilter>()
            .AddTransient<GetPersonHelper>()
            .AddEvidenceFilesHttpClient(configuration);

        if (environment.IsProduction())
        {
            services
                .AddStartupTask<ReferenceDataCache>()
                .AddRateLimiting(configuration);
        }

        if (!environment.IsTests() && !environment.IsEndToEndTests())
        {
            services.AddIdentityApi(configuration);
        }

        return services;
    }

    private static IServiceCollection AddAutoMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg =>
            {
                cfg.AddMaps(typeof(Program).Assembly);
                cfg.CreateMap(typeof(Option<>), typeof(Option<>)).ConvertUsing(typeof(OptionToOptionTypeConverter<,>));
                cfg.CreateMap(typeof(OneOf<,>), typeof(OneOf<,>)).ConvertUsing(typeof(OneOfToOneOfTypeConverter<,,,>));
            })
            .AddTransient(typeof(WrapWithOptionValueConverter<>))
            .AddTransient(typeof(WrapWithOptionValueConverter<,>));

        return services;
    }

    private static IServiceCollection AddEvidenceFilesHttpClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<EvidenceFilesOptions>()
            .Bind(configuration.GetSection("EvidenceFiles"))
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

        return services;
    }

    private static IServiceCollection AddFluentValidation(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation(options => options.DisableDataAnnotationsValidation = true)
            .AddValidatorsFromAssemblyContaining(typeof(Program))
            .AddTransient<IValidatorInterceptor, PreferModelBindingErrorsValidationInterceptor>();

        return services;
    }
}
