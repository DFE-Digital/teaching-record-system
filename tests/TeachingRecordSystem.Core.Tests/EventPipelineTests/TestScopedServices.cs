using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using TeachingRecordSystem.TestCommon.Infrastructure;

namespace TeachingRecordSystem.Core.Tests.EventPipelineTests;

public class TestScopedServices
{
    private static readonly AsyncLocal<TestScopedServices> _current = new();

    public TestScopedServices()
    {
        Clock = new FakeTimeProvider(new DateTimeOffset(2021, 1, 4, 0, 0, 0, TimeSpan.Zero));
        Events = new(Clock);
        LegacyEventObserver = new();
    }

    public FakeTimeProvider Clock { get; }

    public EventCapture Events { get; }

    public CaptureEventObserver LegacyEventObserver { get; }

    public static void ConfigureServices(IServiceCollection services) =>
        services
            .AddSingleton<TimeProvider>(new ForwardToTestScopedTimeProvider())
            .AddTestScoped<EventCapture>(tss => tss.Events)
            .AddTransient<IEventHandler>(sp => sp.GetRequiredService<EventCapture>())
            .AddSingleton<IEventObserver>(new ForwardToTestScopedEventObserver());

    public static TestScopedServices GetCurrent() =>
        TryGetCurrent(out var current) ? current : throw new InvalidOperationException("No current instance has been set.");

    public static TestScopedServices Reset()
    {
        if (_current.Value is not null)
        {
            throw new InvalidOperationException("Current instance has already been set.");
        }

        return _current.Value = new();
    }

    public static bool TryGetCurrent([NotNullWhen(true)] out TestScopedServices? current)
    {
        if (_current.Value is TestScopedServices tss)
        {
            current = tss;
            return true;
        }

        current = default;
        return false;
    }

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
