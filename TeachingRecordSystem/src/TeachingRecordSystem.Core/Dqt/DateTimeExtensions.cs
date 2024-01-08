namespace TeachingRecordSystem.Core.Dqt;

public static class DateTimeExtensions
{
    private static readonly TimeZoneInfo _gmt = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");

    public static DateTime ToDateTimeWithDqtBstFix(this DateOnly dateOnly, bool isLocalTime)
    {
        var dt = dateOnly.ToDateTime(new TimeOnly());
        return isLocalTime ? TimeZoneInfo.ConvertTimeFromUtc(dt, _gmt) : dt;
    }

    public static DateTime? ToDateTimeWithDqtBstFix(this DateOnly? dateOnly, bool isLocalTime) =>
        dateOnly.HasValue ? ToDateTimeWithDqtBstFix(dateOnly.Value, isLocalTime) : null;

    public static DateOnly ToDateOnlyWithDqtBstFix(this DateTime dateTime, bool isLocalTime) =>
        DateOnly.FromDateTime(isLocalTime ? TimeZoneInfo.ConvertTimeFromUtc(dateTime, _gmt) : dateTime);

    public static DateOnly? ToDateOnlyWithDqtBstFix(this DateTime? dateTime, bool isLocalTime) =>
        dateTime.HasValue ? ToDateOnlyWithDqtBstFix(dateTime.Value, isLocalTime) : null;

    public static DateTime? ToUtc(this DateTime dateTime) =>
        TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified), _gmt);

    public static DateTime? ToUtc(this DateTime? dateTime) =>
        dateTime.HasValue ? ToUtc(dateTime.Value) : null;

    public static DateTime ToLocal(this DateTime dateTime) =>
        TimeZoneInfo.ConvertTimeFromUtc(dateTime, _gmt);

    public static DateTime? ToLocal(this DateTime? dateTime) =>
        dateTime.HasValue ? ToLocal(dateTime.Value) : null;
}
