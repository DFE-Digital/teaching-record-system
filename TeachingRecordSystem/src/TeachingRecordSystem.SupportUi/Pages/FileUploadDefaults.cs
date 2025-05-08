namespace TeachingRecordSystem.SupportUi.Pages;

public static class FileUploadDefaults
{
    public const int MaxFileUploadSizeMb = 50;
    public const int DetailMaxCharacterCount = 4000;
    public const int DetailTextAreaMinimumRows = 10;
    public const string MaxFileUploadSizeErrorMessage = "must be smaller than 50MB";
    public const string DetailMaxCharacterCountErrorMessage = "must be 4000 characters or less";
    public static TimeSpan FileUrlExpiry { get; } = TimeSpan.FromMinutes(15);
}
