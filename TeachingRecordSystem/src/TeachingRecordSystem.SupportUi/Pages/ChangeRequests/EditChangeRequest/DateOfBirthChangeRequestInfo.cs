namespace TeachingRecordSystem.SupportUi.Pages.ChangeRequests.EditChangeRequest;

public record DateOfBirthChangeRequestInfo
{
    public required DateOnly CurrentDateOfBirth { get; init; }
    public required DateOnly NewDateOfBirth { get; init; }
}
