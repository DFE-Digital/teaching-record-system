namespace TeachingRecordSystem.Core.Dqt;

public static class DateTimeExtensions
{
    private static readonly TimeZoneInfo _gmt = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");

    public static DateOnly ToDateOnlyWithDqtBstFix(this DateTime dateTime, bool isLocalTime) =>
        DateOnly.FromDateTime(isLocalTime ? TimeZoneInfo.ConvertTimeFromUtc(dateTime, _gmt) : dateTime);

    public static DateOnly? ToDateOnlyWithDqtBstFix(this DateTime? dateTime, bool isLocalTime) =>
        dateTime.HasValue ? ToDateOnlyWithDqtBstFix(dateTime.Value, isLocalTime) : null;
}
