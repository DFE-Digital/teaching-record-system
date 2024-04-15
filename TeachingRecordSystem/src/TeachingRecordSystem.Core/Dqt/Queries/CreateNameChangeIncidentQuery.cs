namespace TeachingRecordSystem.Core.Dqt.Queries;

public record CreateNameChangeIncidentQuery : ICrmQuery<(Guid IncidentId, string TicketNumber)>
{
    public required Guid ContactId { get; init; }
    public required string FirstName { get; init; }
    public required string? MiddleName { get; init; }
    public required string LastName { get; init; }
    public required string StatedFirstName { get; init; }
    public required string? StatedMiddleName { get; init; }
    public required string StatedLastName { get; init; }
    public required string EvidenceFileName { get; init; }
    public required Stream EvidenceFileContent { get; init; }
    public required string EvidenceFileMimeType { get; init; }
    public required bool FromIdentity { get; init; }
}
