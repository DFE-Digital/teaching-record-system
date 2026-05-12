using Sentry.Extensibility;

namespace TeachingRecordSystem.AuthorizeAccess.Infrastructure;

public class DropEventProcessor : ISentryEventProcessor
{
    private DropEventProcessor()
    {
    }

    public static DropEventProcessor Instance { get; } = new();

    public SentryEvent? Process(SentryEvent @event) => null;
}
