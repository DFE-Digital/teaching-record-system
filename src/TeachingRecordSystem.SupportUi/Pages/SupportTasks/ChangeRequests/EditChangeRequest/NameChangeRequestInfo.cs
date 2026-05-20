namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.ChangeRequests.EditChangeRequest;

public record NameChangeRequestInfo
{
    public required string CurrentFirstName { get; init; }
    public required string? CurrentMiddleName { get; init; }
    public required string CurrentLastName { get; init; }
    public required string NewFirstName { get; init; }
    public required string? NewMiddleName { get; init; }
    public required string NewLastName { get; init; }
}
