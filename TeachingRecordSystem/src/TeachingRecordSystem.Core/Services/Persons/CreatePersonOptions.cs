namespace TeachingRecordSystem.Core.Services.Persons;

public record CreatePersonOptions
{
    public required PersonDetails PersonDetails { get; init; }
    public required Guid UserId { get; init; }
    public required Justification<PersonCreateReason> Justification { get; init; }
}
