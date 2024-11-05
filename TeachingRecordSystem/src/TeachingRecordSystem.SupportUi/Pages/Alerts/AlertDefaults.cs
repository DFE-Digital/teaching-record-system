namespace TeachingRecordSystem.SupportUi.Pages.Alerts;

public static class AlertDefaults
{
    public const int MaxFileUploadSizeMb = 50;
    public const int DetailMaxCharacterCount = 4000;
    public const int DetailTextAreaMinimumRows = 10;
    public static TimeSpan FileUrlExpiry { get; } = TimeSpan.FromMinutes(15);
}
