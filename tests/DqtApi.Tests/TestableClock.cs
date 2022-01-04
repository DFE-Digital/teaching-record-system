using System;

namespace DqtApi.Tests
{
    public class TestableClock : IClock
    {
        public static DateTime Initial => new(2021, 1, 4);  // Arbitary start date

        public DateTime UtcNow { get; set; } = Initial;

        public void Reset() => UtcNow = Initial;
    }
}
