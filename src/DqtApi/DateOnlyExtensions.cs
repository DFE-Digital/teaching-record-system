using System;

namespace DqtApi
{
    public static class DateOnlyExtensions
    {
        public static DateTime ToDateTime(this DateOnly dateOnly) => dateOnly.ToDateTime(new());
    }
}
