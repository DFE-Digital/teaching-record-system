namespace TeachingRecordSystem.Core.Services.Persons;

public record SetPersonInductionStatusRequest
{
    public required Guid PersonId { get; init; }
    public required InductionStatus InductionStatus { get; init; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? CompletedDate { get; set; }
    public Guid[]? ExemptionReasonIds { get; set; } = [];
    public required Guid UserId { get; init; }
    public required Justification<PersonInductionChangeReason> Justification { get; init; }
}
