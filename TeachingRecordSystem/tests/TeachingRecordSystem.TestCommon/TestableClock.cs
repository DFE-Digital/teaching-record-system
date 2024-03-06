namespace TeachingRecordSystem.TestCommon;

public class TestableClock : IClock
{
    private static readonly TimeZoneInfo _gmt = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");

    public static DateTime Initial => new(2021, 1, 4, 0, 0, 0, 0, DateTimeKind.Utc);  // Arbitary start date

    public DateTime UtcNow { get; set; } = Initial;
    public DateOnly Today => DateOnly.FromDateTime(UtcNow);
    public DateTime NowGmt => TimeZoneInfo.ConvertTimeFromUtc(UtcNow, _gmt);

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
