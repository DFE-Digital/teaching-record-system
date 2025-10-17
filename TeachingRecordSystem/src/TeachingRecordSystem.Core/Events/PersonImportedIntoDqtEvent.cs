namespace TeachingRecordSystem.Core.Events;

public record PersonImportedIntoDqtEvent : IEvent
{
    public required Guid EventId { get; init; }
    public Guid[] PersonIds => [PersonId];
    public required Guid PersonId { get; init; }
    public required string? Trn { get; init; }
    public required string FirstName { get; set; }
    public required string MiddleName { get; set; }
    public required string LastName { get; set; }
    public required DateOnly? DateOfBirth { get; set; }
    public required string? EmailAddress { get; set; }
    public required string? NationalInsuranceNumber { get; set; }
    public required Gender? Gender { get; set; }
    public required DateOnly? DateOfDeath { get; init; }
    public required DateOnly? QtsDate { get; init; }
    public required DateOnly? EytsDate { get; init; }
    public required InductionStatus? InductionStatus { get; init; }
    public required string? DqtInductionStatus { get; init; }
}
