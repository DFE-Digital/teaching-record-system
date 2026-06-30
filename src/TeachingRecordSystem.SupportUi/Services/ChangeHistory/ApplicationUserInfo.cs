namespace TeachingRecordSystem.SupportUi.Services.ChangeHistory;

public record ApplicationUserInfo
{
    public required string Name { get; init; }
    public required string ShortName { get; init; }
}
