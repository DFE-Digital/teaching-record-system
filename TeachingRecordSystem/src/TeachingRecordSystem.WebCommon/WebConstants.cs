namespace TeachingRecordSystem.WebCommon;

public static class WebConstants
{
    public const string EmptyFallbackContent = "Not provided";
    public const string DateOnlyDisplayFormat = DateDisplayFormat;
    public const string DateTimeDisplayFormat = "d MMM yyyy h:mm:ss tt";

    public static TimeSpan FileUrlExpiry { get; } = TimeSpan.FromMinutes(15);
}
