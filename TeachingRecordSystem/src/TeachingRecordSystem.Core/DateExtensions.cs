namespace TeachingRecordSystem.Core;

public static class DateExtensions
{
    private static readonly TimeZoneInfo _gmt = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");

    public static DateTime ToDateTime(this DateOnly dateOnly) => dateOnly.ToDateTime(new(), DateTimeKind.Utc);

    public static DateTime? ToDateTime(this DateOnly? dateOnly) =>
        dateOnly.HasValue ? dateOnly.Value.ToDateTime(new(), DateTimeKind.Utc) : null;

    public static DateTime ToGmt(this DateTime dateTime) =>
        TimeZoneInfo.ConvertTimeFromUtc(dateTime, _gmt);
}
