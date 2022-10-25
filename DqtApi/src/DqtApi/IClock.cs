﻿using System;

namespace DqtApi
{
    public interface IClock
    {
        DateTime UtcNow { get; }
        DateOnly Today => DateOnly.FromDateTime(UtcNow);
    }
}
