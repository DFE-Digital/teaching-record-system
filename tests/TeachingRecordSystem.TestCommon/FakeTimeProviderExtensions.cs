using Microsoft.Extensions.Time.Testing;

namespace TeachingRecordSystem.TestCommon;

public static class FakeTimeProviderExtensions
{
    private static readonly TimeZoneInfo _gmt = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");

    extension(FakeTimeProvider timeProvider)
    {
        public DateTime NowGmt => TimeZoneInfo.ConvertTimeFromUtc(timeProvider.UtcNow, _gmt);

        public DateTime Advance()
        {
            timeProvider.Advance(TimeSpan.FromSeconds(Random.Shared.Next(1, 86400)));
            return timeProvider.UtcNow;
        }
    }
}
