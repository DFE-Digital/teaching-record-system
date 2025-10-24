using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.Tests;

[assembly: AssemblyFixture(typeof(CoreFixture))]

namespace TeachingRecordSystem.Core.Tests;

public sealed class CoreFixture() : DbFixture(CreateServiceProvider()), IAsyncLifetime
{
    private static readonly FakeTrnGenerator _trnGenerator = new();

    public TestableClock Clock => Services.GetRequiredService<TestableClock>();

    public TestData TestData => Services.GetRequiredService<TestData>();

    public static void AddCoreServices(IServiceCollection services)
    {
        var testConfiguration = new ConfigurationBuilder()
            .AddUserSecrets<CoreFixture>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        DbHelper.ConfigureDbServices(services, testConfiguration.GetPostgresConnectionString());

        services.AddSingleton<IConfiguration>(testConfiguration);
        services.AddSingleton(_trnGenerator);
        services.AddSingleton(new TestableClock());
        services.AddSingleton<IClock>(sp => sp.GetRequiredService<TestableClock>());
        services.AddSingleton<ReferenceDataCache>();
        services.AddSingleton<TestData>();
    }

    private static IServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        AddCoreServices(services);
        return services.BuildServiceProvider();
    }

    async ValueTask IAsyncLifetime.InitializeAsync() => await DbHelper.ClearDataAsync();

    ValueTask IAsyncDisposable.DisposeAsync() => ValueTask.CompletedTask;
}
