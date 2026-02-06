using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.TestCommon.Infrastructure;

namespace TeachingRecordSystem.Core.Tests.EventPipelineTests;

public class TestScopedServices
{
    private static readonly AsyncLocal<TestScopedServices> _current = new();

    public TestScopedServices()
    {
        Clock = new();
        Events = new(Clock);
        LegacyEventObserver = new();
    }

    public TestableClock Clock { get; }

    public EventCapture Events { get; }

    public CaptureEventObserver LegacyEventObserver { get; }

    public static void ConfigureServices(IServiceCollection services) =>
        services
            .AddSingleton<IClock>(new ForwardToTestScopedClock())
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
