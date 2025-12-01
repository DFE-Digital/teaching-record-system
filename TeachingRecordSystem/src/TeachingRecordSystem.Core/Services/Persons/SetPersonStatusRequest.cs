namespace TeachingRecordSystem.Core.Services.Persons;

public record SetPersonStatusRequest
{
    public required Guid PersonId { get; init; }
    public required PersonStatus TargetStatus { get; init; }
    public required Guid UserId { get; init; }
    public required Justification<PersonDeactivateReason>? DeactivateJustification { get; init; }
    public required Justification<PersonReactivateReason>? ReactivateJustification { get; init; }
}
