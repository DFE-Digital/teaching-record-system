using Microsoft.Extensions.Time.Testing;
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
        Clock = new FakeTimeProvider(new DateTimeOffset(2021, 1, 4, 0, 0, 0, TimeSpan.Zero));
        Events = new(Clock);
        LegacyEventObserver = new();
        GetAnIdentityApiClient = new();
        BlobStorageFileService = new();
        SafeFileService = new();
        SafeFileService
            .Setup(s => s.TrySafeUploadAsync(
                It.IsAny<Stream>(),
                It.IsAny<string?>(),
                out It.Ref<Guid>.IsAny,
                null))
            .Callback((Stream stream, string? contentType, out Guid fileId, Guid? fileIdOverride) =>
            {
                fileId = fileIdOverride ?? Guid.NewGuid();
            })
            .ReturnsAsync(true);
        BackgroundJobScheduler = new(serviceProvider);
    }

    public static void ConfigureServices(IServiceCollection services) =>
        services
            .AddSingleton<TimeProvider>(new ForwardToTestScopedTimeProvider())
            .AddSingleton<IEventObserver>(new ForwardToTestScopedEventObserver())
            .AddTestScoped(tss => tss.GetAnIdentityApiClient.Object)
            .AddTestScoped(tss => tss.Events)
            .AddTestScoped(tss => tss.SafeFileService.Object)
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

    public FakeTimeProvider Clock { get; }

    public EventCapture Events { get; }

    public CaptureEventObserver LegacyEventObserver { get; }

    public Mock<IGetAnIdentityApiClient> GetAnIdentityApiClient { get; }

    public Mock<IFileService> BlobStorageFileService { get; }

    public Mock<ISafeFileService> SafeFileService { get; }

    public DeferredExecutionBackgroundJobScheduler BackgroundJobScheduler { get; }

    private class ForwardToTestScopedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => GetCurrent().Clock.GetUtcNow();
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
