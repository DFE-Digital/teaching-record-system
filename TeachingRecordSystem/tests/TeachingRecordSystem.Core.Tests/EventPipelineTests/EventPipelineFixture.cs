using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.Tests.EventPipelineTests;
using TeachingRecordSystem.TestCommon.Infrastructure;

[assembly: AssemblyFixture(typeof(EventPipelineFixture))]

namespace TeachingRecordSystem.Core.Tests.EventPipelineTests;

public class EventPipelineFixture : ServiceProviderFixture
{
    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddSingleton<TestData>()
            .AddSingleton<ReferenceDataCache>()
            .AddEventPublisher();

        PublishEventsDbCommandInterceptor.ConfigureServices(services);

        TestScopedServices.ConfigureServices(services);
    }
}
