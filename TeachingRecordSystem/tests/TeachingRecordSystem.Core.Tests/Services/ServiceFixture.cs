using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.Services.Notify;
using TeachingRecordSystem.Core.Services.OneLogin;
using TeachingRecordSystem.Core.Services.SupportTasks;
using TeachingRecordSystem.Core.Tests.Services;

[assembly: AssemblyFixture(typeof(ServiceFixture))]

namespace TeachingRecordSystem.Core.Tests.Services;

public class ServiceFixture : ServiceProviderFixture
{
    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddSingleton<TestData>()
            .AddSingleton<ReferenceDataCache>()
            .AddEventPublisher()
            .AddOneLoginService()
            .AddSupportTaskService()
            .AddSingleton<INotificationSender, NoopNotificationSender>();

        TestScopedServices.ConfigureServices(services);
    }
}
