using System;

namespace QualifiedTeachersApi;

public static class DateOnlyExtensions
{
    public static DateTime ToDateTime(this DateOnly dateOnly) => dateOnly.ToDateTime(new());

    public static DateTime? ToDateTime(this DateOnly? dateOnly) =>
        dateOnly.HasValue ? dateOnly.Value.ToDateTime(new()) : null;

    public static DateOnly ToDateOnly(this DateTime dateTime) => DateOnly.FromDateTime(dateTime);

    public static DateOnly? ToDateOnly(this DateTime? dateTime) =>
        dateTime.HasValue ? DateOnly.FromDateTime(dateTime.Value) : null;
}
