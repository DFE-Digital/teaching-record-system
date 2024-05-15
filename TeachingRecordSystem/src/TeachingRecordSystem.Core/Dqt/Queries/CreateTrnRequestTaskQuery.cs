namespace TeachingRecordSystem.Core.Dqt.Queries;

public record CreateTrnRequestTaskQuery : ICrmQuery<Guid>
{
    public required string Description { get; init; }
    public required string EvidenceFileName { get; init; }
    public required Stream EvidenceFileContent { get; init; }
    public required string EvidenceFileMimeType { get; init; }
}
