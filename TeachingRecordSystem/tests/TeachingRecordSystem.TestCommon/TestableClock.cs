namespace TeachingRecordSystem.TestCommon;

public class TestableClock : IClock
{
    public static DateTime Initial => new(2021, 1, 4, 0, 0, 0, 0, DateTimeKind.Utc);  // Arbitary start date

    public DateTime UtcNow { get; set; } = Initial;
    public DateOnly Today => DateOnly.FromDateTime(UtcNow);

    public DateTime Advance() => Advance(TimeSpan.FromDays(1));

    public DateTime Advance(TimeSpan ts)
    {
        if (ts < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(ts));
        }

        return UtcNow += ts;
    }

    public void Reset() => UtcNow = Initial;
}
