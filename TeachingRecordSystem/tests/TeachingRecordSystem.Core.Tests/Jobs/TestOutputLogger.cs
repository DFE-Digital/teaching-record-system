using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class TestOutputLogger<T>(ITestOutputHelper outputHelper) : ILogger<T>
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => new Scope();

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        outputHelper.WriteLine($"{logLevel}: {formatter(state, exception)}");
    }

    public sealed class Scope : IDisposable
    {
        public void Dispose()
        {
        }
    }
}
