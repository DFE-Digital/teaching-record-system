namespace TeachingRecordSystem.Core;

public static class DateOnlyExtensions
{
    public static DateTime ToDateTime(this DateOnly dateOnly) => dateOnly.ToDateTime(new(), DateTimeKind.Utc);

    public static DateTime? ToDateTime(this DateOnly? dateOnly) =>
        dateOnly.HasValue ? dateOnly.Value.ToDateTime(new(), DateTimeKind.Utc) : null;
}
