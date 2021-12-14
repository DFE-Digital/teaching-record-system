using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json.Serialization;
using DqtApi.Configuration;
using DqtApi.DataStore.Crm;
using DqtApi.DataStore.Sql;
using DqtApi.Filters;
using DqtApi.ModelBinding;
using DqtApi.Security;
using DqtApi.Swagger;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.PowerPlatform.Dataverse.Client;
using Npgsql;
using Prometheus;
using Serilog;
using Serilog.Context;
using Swashbuckle.AspNetCore.Filters;

namespace DqtApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((ctx, config) => config.ReadFrom.Configuration(ctx.Configuration));

            if (builder.Environment.IsProduction())
            {
                builder.Configuration
                    .AddJsonEnvironmentVariable("AppConfig")
                    .AddJsonEnvironmentVariable("VCAP_SERVICES", configurationKeyPrefix: "VCAP_SERVICES")
                    .AddJsonEnvironmentVariable("VCAP_APPLICATION", configurationKeyPrefix: "VCAP_APPLICATION");
            }

            var services = builder.Services;
            var env = builder.Environment;
            var configuration = builder.Configuration;

            services.AddAuthentication(ApiKeyAuthenticationHandler.AuthenticationScheme)
                .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationHandler.AuthenticationScheme, _ => { });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(
                    "Bearer",
                    policy => policy
                        .AddAuthenticationSchemes(ApiKeyAuthenticationHandler.AuthenticationScheme)
                        .RequireClaim(ClaimTypes.Name));

                options.DefaultPolicy = options.GetPolicy("Bearer");
            });

            services
                .AddMvc(options =>
                {
                    options.AddHybridBodyModelBinderProvider();

                    options.Filters.Add(new AuthorizeFilter());
                    options.Filters.Add(new ProducesJsonOrProblemAttribute());
                    options.Filters.Add(new CrmServiceProtectionFaultExceptionFilter());

                    options.Conventions.Add(new ApiVersionConvention());
                })
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.SuppressInferBindingSourcesForParameters = true;
                })
                .AddFluentValidation(fv =>
                {
                    fv.RegisterValidatorsFromAssemblyContaining(typeof(Program));
                });

            services.AddTransient<IApiDescriptionProvider, HybridBodyApiDescriptionProvider>();

            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo() { Title = "DQT API", Version = "v1" });
                c.SwaggerDoc("v2", new OpenApiInfo() { Title = "DQT API", Version = "v2" });

                c.DocInclusionPredicate((docName, api) => docName.Equals(api.GroupName, StringComparison.OrdinalIgnoreCase));
                c.EnableAnnotations();
                c.ExampleFilters();
                c.OperationFilter<ResponseContentTypeOperationFilter>();

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

            var pgConnectionString = GetPostgresConnectionString();

            var healthCheckBuilder = services.AddHealthChecks()
                .AddNpgSql(pgConnectionString);

            services.AddMediatR(typeof(Program));
            services.AddSingleton<IApiClientRepository, ConfigurationApiClientRepository>();
            services.AddSingleton<ICurrentClientProvider, ClaimsPrincipalCurrentClientProvider>();
            services.AddSwaggerExamplesFromAssemblyOf<Program>();

            services.AddDbContext<DqtContext>(options =>
            {
                DqtContext.ConfigureOptions(options, pgConnectionString);
            });

            services.AddDatabaseDeveloperPageExceptionFilter();

            if (env.EnvironmentName != "Testing")
            {
                var crmServiceClient = GetCrmServiceClient();

                services.AddSingleton<IOrganizationServiceAsync>(crmServiceClient);
                services.AddSingleton<IDataverseAdapter, DataverseAdapter>();

                healthCheckBuilder.AddCheck("CRM", () => crmServiceClient.IsReady ? HealthCheckResult.Healthy() : HealthCheckResult.Degraded());
            }

            MetricLabels.ConfigureLabels(builder.Configuration);

            var app = builder.Build();            

            app.Use((ctx, next) =>
            {
                LogContext.PushProperty("CorrelationId", ctx.TraceIdentifier);
                return next();
            });

            app.UseSerilogRequestLogging();

            app.UseRouting();
            app.UseHttpMetrics();

            app.UseHealthChecks("/status");

            app.UseAuthentication();
            app.UseAuthorization();

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
                    c.EnablePersistAuthorization();
                });

                app.UseMigrationsEndPoint();

                using (var scope = app.Services.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<DqtContext>();
                    context.Database.EnsureCreated();
                }
            }

            app.Run();

            ServiceClient GetCrmServiceClient() =>
                new ServiceClient(
                    new Uri(configuration["CrmUrl"]),
                    configuration["CrmClientId"],
                    configuration["CrmClientSecret"],
                    useUniqueInstance: true);

            string GetPostgresConnectionString()
            {
                return configuration.GetConnectionString("DefaultConnection") ?? GetConnectionStringForPaasService();

                string GetConnectionStringForPaasService()
                {
                    var builder = new NpgsqlConnectionStringBuilder()
                    {
                        Host = configuration.GetValue<string>("VCAP_SERVICES:postgres:0:credentials:host"),
                        Database = configuration.GetValue<string>("VCAP_SERVICES:postgres:0:credentials:name"),
                        Username = configuration.GetValue<string>("VCAP_SERVICES:postgres:0:credentials:username"),
                        Password = configuration.GetValue<string>("VCAP_SERVICES:postgres:0:credentials:password"),
                        Port = configuration.GetValue<int>("VCAP_SERVICES:postgres:0:credentials:port"),
                        SslMode = SslMode.Require,
                        TrustServerCertificate = true
                    };

                    return builder.ConnectionString;
                }
            }
        }
    }
}
