namespace QualifiedTeachersApi;

public static class DateOnlyExtensions
{
    public static DateTime ToDateTime(this DateOnly dateOnly) => dateOnly.ToDateTime(new(), DateTimeKind.Utc);

    public static DateTime? ToDateTime(this DateOnly? dateOnly) =>
        dateOnly.HasValue ? dateOnly.Value.ToDateTime(new(), DateTimeKind.Utc) : null;

    public static DateOnly ToDateOnly(this DateTime dateTime) => DateOnly.FromDateTime(dateTime);

    public static DateOnly? ToDateOnly(this DateTime? dateTime) =>
        dateTime.HasValue ? DateOnly.FromDateTime(dateTime.Value) : null;
}
