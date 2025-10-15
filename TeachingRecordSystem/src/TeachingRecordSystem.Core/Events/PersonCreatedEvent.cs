namespace TeachingRecordSystem.Core.Events;

public record PersonCreatedEvent : IEvent
{
    public required Guid EventId { get; init; }
    public Guid[] PersonIds => [PersonId];
    public required Guid PersonId { get; init; }
    public required string FirstName { get; set; }
    public required string MiddleName { get; set; }
    public required string LastName { get; set; }
    public required DateOnly? DateOfBirth { get; set; }
    public required string? EmailAddress { get; set; }
    public required string? NationalInsuranceNumber { get; set; }
    public required Gender? Gender { get; set; }
    public required string? CreateReason { get; init; }
    public required string? CreateReasonDetail { get; init; }
    public required EventModels.File? EvidenceFile { get; init; }
    public required EventModels.TrnRequestMetadata? TrnRequestMetadata { get; init; }
}
