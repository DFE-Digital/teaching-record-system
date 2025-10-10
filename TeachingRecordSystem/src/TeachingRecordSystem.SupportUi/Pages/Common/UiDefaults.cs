namespace TeachingRecordSystem.SupportUi.Pages.Common;

public static class UiDefaults
{
    public const string EmptyDisplayContent = "Not provided";
    public const string DateOnlyDisplayFormat = "d MMMM yyyy";
    public const string DateTimeDisplayFormat = "d MMM yyyy h:mm:ss tt";
    public const int MaxFileUploadSizeMb = 50;
    public const int DetailMaxCharacterCount = 4000;
    public const int DetailTextAreaMinimumRows = 10;
    public const string MaxFileUploadSizeErrorMessage = "must be smaller than 50MB";
    public const string DetailMaxCharacterCountErrorMessage = "must be 4000 characters or less";
    public static TimeSpan FileUrlExpiry { get; } = TimeSpan.FromMinutes(15);
}
