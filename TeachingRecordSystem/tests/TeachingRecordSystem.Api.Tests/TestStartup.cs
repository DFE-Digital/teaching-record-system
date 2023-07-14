using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.Services.Certificates;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;
using TeachingRecordSystem.Dqt;

namespace TeachingRecordSystem.Api.Tests;

public class TestStartup : ITestStartup
{
    public void ConfigureConfiguration(IConfigurationBuilder builder)
    {
        builder.AddUserSecrets<ApiFixture>(optional: true)
            .AddEnvironmentVariables();
    }

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        DbHelper.ConfigureDbServices(services, configuration.GetRequiredConnectionString("DefaultConnection"));

        services.AddSingleton<ApiFixture>();
        services.AddSingleton<DbFixture>();
        services.AddMemoryCache();
        services.AddScoped<IClock, TestableClock>();
        services.AddScoped<IDataverseAdapter>(_ => Mock.Of<IDataverseAdapter>());
        services.AddScoped<IGetAnIdentityApiClient>(_ => Mock.Of<IGetAnIdentityApiClient>());
        services.AddScoped<IOptions<GetAnIdentityOptions>>(_ => Mock.Of<IOptions<GetAnIdentityOptions>>());
        services.AddScoped<ICertificateGenerator>(_ => Mock.Of<ICertificateGenerator>());
    }

    public async Task Initialize(IServiceProvider services)
    {
        await services.GetRequiredService<DbHelper>().EnsureSchema();
    }
}
