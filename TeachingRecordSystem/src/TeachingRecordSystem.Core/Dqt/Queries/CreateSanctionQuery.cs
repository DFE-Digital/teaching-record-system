namespace TeachingRecordSystem.Core.Dqt.Queries;

public record CreateSanctionQuery : ICrmQuery<Guid>
{
    public required Guid ContactId { get; init; }
    public required Guid SanctionCodeId { get; init; }
    public required string Details { get; init; }
    public required string? Link { get; init; }
    public required DateOnly StartDate { get; init; }
}
