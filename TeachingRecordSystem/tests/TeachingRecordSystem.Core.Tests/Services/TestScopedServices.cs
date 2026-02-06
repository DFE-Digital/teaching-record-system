using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.TestCommon.Infrastructure;

namespace TeachingRecordSystem.Core.Tests.Services;

public class TestScopedServices
{
    private static readonly AsyncLocal<TestScopedServices> _current = new();

    public TestScopedServices(IServiceProvider serviceProvider)
    {
        Clock = new();
        Events = new(Clock);
        BackgroundJobScheduler = new(serviceProvider);
    }

    public TestableClock Clock { get; }

    public EventCapture Events { get; }

    public DeferredExecutionBackgroundJobScheduler BackgroundJobScheduler { get; }

    public static void ConfigureServices(IServiceCollection services) =>
        services
            .AddSingleton<IClock>(new ForwardToTestScopedClock())
            .AddTestScoped<EventCapture>(tss => tss.Events)
            .AddTransient<IEventHandler>(sp => sp.GetRequiredService<EventCapture>())
            .AddTestScoped<IBackgroundJobScheduler>(tss => tss.BackgroundJobScheduler);

    public static TestScopedServices GetCurrent() =>
        TryGetCurrent(out var current) ? current : throw new InvalidOperationException("No current instance has been set.");

    public static TestScopedServices Reset(IServiceProvider serviceProvider)
    {
        if (_current.Value is not null)
        {
            throw new InvalidOperationException("Current instance has already been set.");
        }

        return _current.Value = new(serviceProvider);
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
}

file static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTestScoped<T>(this IServiceCollection services, Func<TestScopedServices, T> resolveService)
        where T : class
    {
        return services.AddTransient(_ => resolveService(TestScopedServices.GetCurrent()));
    }
}
