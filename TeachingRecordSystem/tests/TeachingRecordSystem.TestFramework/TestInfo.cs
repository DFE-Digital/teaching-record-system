namespace TeachingRecordSystem.TestFramework;

public class TestInfo
{
    private static readonly AsyncLocal<TestInfo?> _current = new();

    internal TestInfo(IServiceProvider testServices, TextWriter console)
    {
        TestServices = testServices;
        Console = console;
    }

    public TextWriter Console { get; }

    public IServiceProvider TestServices { get; }

    public static TestInfo Current => _current.Value ?? throw new InvalidOperationException("No current test.");

    internal static void SetCurrent(TestInfo? current) => _current.Value = current;
}
