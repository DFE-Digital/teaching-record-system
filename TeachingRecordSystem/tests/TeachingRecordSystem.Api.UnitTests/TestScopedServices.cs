using System.Diagnostics.CodeAnalysis;

namespace TeachingRecordSystem.Api.UnitTests;

public class TestScopedServices
{
    private static readonly AsyncLocal<TestScopedServices> _current = new();

    public TestScopedServices()
    {
        CrmQueryDispatcherSpy = new();
        Clock = new();
    }

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

    public CrmQueryDispatcherSpy CrmQueryDispatcherSpy { get; }

    public TestableClock Clock { get; }
}
