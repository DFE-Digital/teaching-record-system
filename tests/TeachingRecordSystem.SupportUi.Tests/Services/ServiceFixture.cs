using TeachingRecordSystem.SupportUi.Tests.Services;

[assembly: AssemblyFixture(typeof(ServiceFixture))]

namespace TeachingRecordSystem.SupportUi.Tests.Services;

public class ServiceFixture : ServiceProviderFixture
{
    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddSingleton<TestData>()
            .AddSingleton<ReferenceDataCache>()
            .AddEventPublisher();

        TestScopedServices.ConfigureServices(services);
    }
}
