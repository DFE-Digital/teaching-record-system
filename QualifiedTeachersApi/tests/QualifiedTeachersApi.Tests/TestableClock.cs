namespace QualifiedTeachersApi.Tests;

public class TestableClock : IClock
{
    public static DateTime Initial => new(2021, 1, 4);  // Arbitary start date

    public DateTime UtcNow { get; set; } = Initial;
    public DateOnly Today => DateOnly.FromDateTime(UtcNow);

    public void Reset() => UtcNow = Initial;
}
