namespace TeachingRecordSystem.Core;

public static class TimeProviderExtensions
{
    extension(TimeProvider timeProvider)
    {
        public DateTime UtcNow => timeProvider.GetUtcNow().DateTime;

        public DateOnly Today => DateOnly.FromDateTime(timeProvider.GetUtcNow().Date);
    }
}
