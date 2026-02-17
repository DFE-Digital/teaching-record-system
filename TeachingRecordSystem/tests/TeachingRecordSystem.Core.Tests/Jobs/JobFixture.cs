using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.Tests.Jobs;

[assembly: AssemblyFixture(typeof(JobFixture))]

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class JobFixture : ServiceProviderFixture
{
    public TestableClock Clock { get; } = new();

    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddSingleton<TimeProvider>(Clock.TimeProvider)
            .AddSingleton<TestData>()
            .AddSingleton<ReferenceDataCache>()
            .AddEventPublisher()
            .AddSingleton<EventCapture>()
            .AddTransient<IEventHandler>(sp => sp.GetRequiredService<EventCapture>());
    }
}
