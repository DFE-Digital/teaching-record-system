using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers;
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
            .AddSingleton<PersonInfoCache>()
            .AddMemoryCache()
            .AddLogging()
            .AddAuthorizationCore()
            .AddSingleton<IAuthorizationHandler, AlertTypeAuthorizationHandler>()
            .AddEventPublisher();

        TestScopedServices.ConfigureServices(services);
    }
}
