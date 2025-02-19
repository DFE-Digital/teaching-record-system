namespace TeachingRecordSystem.SupportUi.Pages;

public static class FileUploadDefaults
{
    public const int MaxFileUploadSizeMb = 50;
    public const int DetailMaxCharacterCount = 4000;
    public const int DetailTextAreaMinimumRows = 10;
    public static TimeSpan FileUrlExpiry { get; } = TimeSpan.FromMinutes(15);
}
