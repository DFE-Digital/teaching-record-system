namespace TeachingRecordSystem.SupportUi.Pages.EditUser;

public static class DeactivateDefaults
{
    public const int MaxFileUploadSizeMb = 100;
    public const int DetailTextAreaMinimumRows = 5;
    public static TimeSpan FileUrlExpiry { get; } = TimeSpan.FromMinutes(15);
}
