using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;
using TeachingRecordSystem.Core.Services.NameSynonyms;
using TeachingRecordSystem.Core.Services.TrnGeneration;

namespace TeachingRecordSystem.Api.UnitTests;

public class Startup
{
    public void ConfigureHost(IHostBuilder hostBuilder)
    {
        hostBuilder
            .ConfigureHostConfiguration(builder => builder
                .AddUserSecrets<Startup>(optional: true)
                .AddEnvironmentVariables())
            .ConfigureServices((context, services) =>
            {
                var pgConnectionString = new NpgsqlConnectionStringBuilder(context.Configuration.GetRequiredConnectionString("DefaultConnection"))
                {
                    // We rely on error details to get the offending duplicate key values in the TrsDataSyncHelper
                    IncludeErrorDetail = true
                }.ConnectionString;

                DbHelper.ConfigureDbServices(services, pgConnectionString);

                services
                    .AddSingleton<DbFixture>()
                    .AddSingleton<OperationTestFixture>()
                    .AddTrsBaseServices()
                    .AddTestScoped<IClock>(tss => tss.Clock)
                    .AddSingleton<FakeTrnGenerator>()
                    .AddSingleton<ITrnGenerator>(sp => sp.GetRequiredService<FakeTrnGenerator>())
                    .AddCrmQueries()
                    .AddFakeXrm()
                    .Decorate<ICrmQueryDispatcher>(
                        inner => new CrmQueryDispatcherDecorator(
                            inner,
                            TestScopedServices.TryGetCurrent(out var tss) ? tss.CrmQueryDispatcherSpy : new()))
                    .AddSingleton<ICurrentUserProvider>(Mock.Of<ICurrentUserProvider>())
                    .AddNameSynonyms()
                    .AddTestScoped<IGetAnIdentityApiClient>(tss => tss.GetAnIdentityApiClient.Object);
            });
    }
}

file static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTestScoped<T>(this IServiceCollection services, Func<TestScopedServices, T> resolveService)
        where T : class
    {
        return services.AddTransient<T>(_ => resolveService(TestScopedServices.GetCurrent()));
    }
}
