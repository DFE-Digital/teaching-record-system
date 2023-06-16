namespace TeachingRecordSystem.Api;

public sealed class Clock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
