using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;
using TeachingRecordSystem.TestCommon.Infrastructure;

namespace TeachingRecordSystem.AuthorizeAccess.Tests;

public class TestScopedServices
{
    private static readonly AsyncLocal<TestScopedServices> _current = new();

    public TestScopedServices(IServiceProvider serviceProvider)
    {
        Clock = new();
        Events = new();
        LegacyEventObserver = new();
        GetAnIdentityApiClient = new();
        BlobStorageFileService = new();
        BackgroundJobScheduler = new(serviceProvider);
    }

    public static void ConfigureServices(IServiceCollection services) =>
        services
            .AddSingleton<IClock>(new ForwardToTestScopedClock())
            .AddSingleton<IEventObserver>(new ForwardToTestScopedEventObserver())
            .AddTestScoped(tss => tss.GetAnIdentityApiClient.Object)
            .AddTestScoped(tss => tss.Events)
            .AddTransient<IEventHandler>(sp => sp.GetRequiredService<EventCapture>())
            .AddTestScoped<IBackgroundJobScheduler>(tss => tss.BackgroundJobScheduler);

    public static TestScopedServices GetCurrent() =>
        _current.Value ?? throw new InvalidOperationException("No current instance has been set.");

    public static TestScopedServices Reset(IServiceProvider serviceProvider)
    {
        if (_current.Value is not null)
        {
            throw new InvalidOperationException("Current instance has already been set.");
        }

        return _current.Value = new(serviceProvider);
    }

    public TestableClock Clock { get; }

    public EventCapture Events { get; }

    public CaptureEventObserver LegacyEventObserver { get; }

    public Mock<IGetAnIdentityApiClient> GetAnIdentityApiClient { get; }

    public Mock<IFileService> BlobStorageFileService { get; }

    public DeferredExecutionBackgroundJobScheduler BackgroundJobScheduler { get; }

    private class ForwardToTestScopedClock : IClock
    {
        public DateTime UtcNow => GetCurrent().Clock.UtcNow;
    }

    private class ForwardToTestScopedEventObserver : IEventObserver
    {
        public void OnEventCreated(LegacyEvents.EventBase @event) => GetCurrent().LegacyEventObserver.OnEventCreated(@event);
    }
}

file static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTestScoped<T>(this IServiceCollection services, Func<TestScopedServices, T> resolveService)
        where T : class
    {
        return services.AddTransient(_ => resolveService(TestScopedServices.GetCurrent()));
    }
}
