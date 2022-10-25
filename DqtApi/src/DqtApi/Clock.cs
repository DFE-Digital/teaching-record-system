using System;

namespace DqtApi
{
    public sealed class Clock : IClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
