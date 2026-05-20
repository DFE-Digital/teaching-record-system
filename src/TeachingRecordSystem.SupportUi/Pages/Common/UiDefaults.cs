namespace TeachingRecordSystem.SupportUi.Pages.Common;

public static class UiDefaults
{
    // File uploads
    public const int MaxFileUploadSizeMb = 50;
    public const string MaxFileUploadSizeErrorMessage = "must be smaller than 50MB";

    // Change reason details
    public const int ReasonDetailsMaxCharacterCount = 4000;
    public const string ReasonDetailsMaxCharacterCountErrorMessage = "must be 4000 characters or less";
    public const int ReasonDetailsRows = 10;
}
