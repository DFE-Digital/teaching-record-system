using TeachingRecordSystem.Core;

namespace TeachingRecordSystem.TestCommon;

public class TestableClock : IClock
{
    public static DateTime Initial => new(2021, 1, 4, 0, 0, 0, 0, DateTimeKind.Utc);  // Arbitary start date

    public DateTime UtcNow { get; set; } = Initial;
    public DateOnly Today => DateOnly.FromDateTime(UtcNow);

    public void Reset() => UtcNow = Initial;
}
