namespace TeachingRecordSystem.Core.Dqt.Queries;

public record CreateTrnRequestTaskQuery : ICrmQuery<Guid>
{
    public const string TaskSubject = "Notification for TRA Support Team - TRN request";

    public required string Description { get; init; }
    public required string EvidenceFileName { get; init; }
    public required Stream EvidenceFileContent { get; init; }
    public required string EvidenceFileMimeType { get; init; }
    public required string EmailAddress { get; init; }
}
