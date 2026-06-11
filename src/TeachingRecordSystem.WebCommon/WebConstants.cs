namespace TeachingRecordSystem.WebCommon;

public static class WebConstants
{
    public const string EmptyFallbackContent = "Not provided";
    public const string DateDisplayFormat = CoreConstants.DateDisplayFormat;
    public const string DateShortDisplayFormat = CoreConstants.DateShortDisplayFormat;
    public const string DateAndTimeDisplayFormat = "d MMM yyyy h:mm:ss tt";

    public static TimeSpan FileUrlExpiry { get; } = TimeSpan.FromMinutes(15);
}
