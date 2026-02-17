using Microsoft.Extensions.Time.Testing;

namespace TeachingRecordSystem.TestCommon;

public class TestableClock
{
    private static readonly TimeZoneInfo _gmt = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
    private static readonly DateTime _initial = new(2021, 1, 4, 0, 0, 0, 0, DateTimeKind.Utc);  // Arbitrary start date

    private readonly FakeTimeProvider _fakeTimeProvider;

    public static DateTime Initial => _initial;

    public TestableClock()
    {
        _fakeTimeProvider = new FakeTimeProvider(_initial);
    }

    public TimeProvider TimeProvider => _fakeTimeProvider;

    public DateTime UtcNow
    {
        get => _fakeTimeProvider.GetUtcNow().DateTime;
        set => _fakeTimeProvider.SetUtcNow(value);
    }

    public DateOnly Today => DateOnly.FromDateTime(UtcNow);
    public DateTime NowGmt => TimeZoneInfo.ConvertTimeFromUtc(UtcNow, _gmt);

    public DateTime Advance() => Advance(TimeSpan.FromDays(1));

    public DateTime Advance(TimeSpan ts)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(ts, TimeSpan.Zero);

        _fakeTimeProvider.Advance(ts);
        return UtcNow;
    }

    public void Reset() => _fakeTimeProvider.SetUtcNow(_initial);
}
