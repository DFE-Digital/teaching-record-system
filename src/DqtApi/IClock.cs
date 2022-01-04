using System;

namespace DqtApi
{
    public interface IClock
    {
        DateTime UtcNow { get; }
    }
}
