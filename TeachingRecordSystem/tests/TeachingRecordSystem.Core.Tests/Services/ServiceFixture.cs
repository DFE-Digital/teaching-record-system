using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.Services.Notify;
using TeachingRecordSystem.Core.Services.OneLogin;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.Core.Services.SupportTasks;
using TeachingRecordSystem.Core.Tests.Services;
using TeachingRecordSystem.Core.Tests.Services.Webhooks;

[assembly: AssemblyFixture(typeof(ServiceFixture))]

namespace TeachingRecordSystem.Core.Tests.Services;

public class ServiceFixture : ServiceProviderFixture
{
    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<IdDbContext>(options => options.UseInMemoryDatabase("TeacherAuthId"), contextLifetime: ServiceLifetime.Transient);

        services
            .AddMemoryCache()
            .AddSingleton<TestData>()
            .AddSingleton<ReferenceDataCache>()
            .AddSingleton<PersonInfoCache>()
            .AddEventPublisher()
            .AddOneLoginService()
            .AddSupportTaskService()
            .AddPersonService()
            .AddSingleton<INotificationSender, NoopNotificationSender>()
            .AddTestTrnGeneration()
            .AddSingleton<WebhookReceiver>();

        TestScopedServices.ConfigureServices(services);
    }
}
