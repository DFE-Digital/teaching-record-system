namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.ChangeRequests.EditChangeRequest;

public record DateOfBirthChangeRequestInfo
{
    public required DateOnly CurrentDateOfBirth { get; init; }
    public required DateOnly NewDateOfBirth { get; init; }
}
