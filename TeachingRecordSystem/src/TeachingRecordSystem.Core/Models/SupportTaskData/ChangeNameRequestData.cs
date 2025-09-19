namespace TeachingRecordSystem.Core.Models.SupportTaskData;

[SupportTaskData("6bc82e72-7592-4b05-a4ae-822fb52cad8d")]
public record ChangeNameRequestData
{
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required Guid EvidenceFileId { get; init; }
    public required string EvidenceFileName { get; init; }
    public required string? EmailAddress { get; init; }
    public required SupportRequestOutcome? ChangeRequestOutcome { get; init; }
}
