namespace TeachingRecordSystem.Core.Services.SupportTasks.ChangeRequests;

public record CreateDateOfBirthChangeRequestSupportTaskOptions
{
    public required Guid PersonId { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required Guid EvidenceFileId { get; init; }
    public required string EvidenceFileName { get; init; }
    public required string? EmailAddress { get; init; }
}
