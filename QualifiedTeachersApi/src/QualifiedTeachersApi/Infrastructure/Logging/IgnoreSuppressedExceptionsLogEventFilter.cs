﻿using Serilog.Core;
using Serilog.Events;

namespace QualifiedTeachersApi.Infrastructure.Logging;

public class IgnoreSuppressedExceptionsLogEventFilter : ILogEventFilter
{
    public bool IsEnabled(LogEvent logEvent)
    {
        if (logEvent.Exception is not null && LogSuppressions.ShouldIgnoreException(logEvent.Exception))
        {
            return false;
        }

        return true;
    }
}
