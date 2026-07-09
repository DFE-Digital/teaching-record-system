namespace TeachingRecordSystem.Core.Services.SupportTasks.ChangeRequests;

public record CreateNameChangeRequestSupportTaskOptions
{
    public required Guid PersonId { get; init; }
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required Guid EvidenceFileId { get; init; }
    public required string EvidenceFileName { get; init; }
    public required string? EmailAddress { get; init; }
}
