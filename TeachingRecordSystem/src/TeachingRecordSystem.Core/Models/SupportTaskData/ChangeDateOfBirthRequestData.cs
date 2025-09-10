namespace TeachingRecordSystem.Core.Models.SupportTaskData;

[SupportTaskData("b621cc79-b116-461e-be8d-593d6efd53cd")]
public record ChangeDateOfBirthRequestData
{
    public required DateOnly DateOfBirth { get; init; }
    public required Guid EvidenceFileId { get; init; }
    public required string EvidenceFileName { get; init; }
    public required string? EmailAddress { get; init; }
    public required SupportRequestOutcome? ChangeRequestOutcome { get; init; }
}
