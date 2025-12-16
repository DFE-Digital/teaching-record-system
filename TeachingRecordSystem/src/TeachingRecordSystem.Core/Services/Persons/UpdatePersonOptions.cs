namespace TeachingRecordSystem.Core.Services.Persons;

public record UpdatePersonOptions
{
    public required Guid PersonId { get; init; }
    public required PersonDetails PersonDetails { get; init; }
    public required Guid UserId { get; init; }
    public required Justification<PersonNameChangeReason>? NameChangeJustification { get; init; }
    public required Justification<PersonDetailsChangeReason>? DetailsChangeJustification { get; init; }
}
