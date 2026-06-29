using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Time.Testing;

namespace TeachingRecordSystem.SupportUi.Tests.Services;

public class TestScopedServices
{
    private static readonly AsyncLocal<TestScopedServices> _current = new();

    public TestScopedServices()
    {
        TimeProvider = new FakeTimeProvider(new DateTimeOffset(2021, 1, 4, 0, 0, 0, TimeSpan.Zero));
        Events = new(TimeProvider);
    }

    public FakeTimeProvider TimeProvider { get; }

    public EventCapture Events { get; }

    public static void ConfigureServices(IServiceCollection services) =>
        services
            .AddSingleton<TimeProvider>(new ForwardToTestScopedTimeProvider())
            .AddSingleton<EventCapture>()
            .AddTransient<IEventHandler>(sp => sp.GetRequiredService<EventCapture>());

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
        public override DateTimeOffset GetUtcNow() => GetCurrent().TimeProvider.GetUtcNow();
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
