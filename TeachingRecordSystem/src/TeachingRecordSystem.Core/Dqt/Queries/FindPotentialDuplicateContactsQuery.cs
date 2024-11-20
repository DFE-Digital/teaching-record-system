namespace TeachingRecordSystem.Core.Dqt.Queries;

public record FindPotentialDuplicateContactsQuery : ICrmQuery<FindPotentialDuplicateContactsResult[]>
{
    public required IEnumerable<string> FirstNames { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required IEnumerable<string> EmailAddresses { get; init; }
    public required string? NationalInsuranceNumber { get; init; }

    // A collection of contact IDs that have already been identified to match on NINO (through workforce data)
    public required Guid[] MatchedOnNationalInsuranceNumberContactIds { get; init; }
}

public record FindPotentialDuplicateContactsResult
{
    public required Guid ContactId { get; init; }
    public required string Trn { get; init; }
    public required IReadOnlyCollection<string> MatchedAttributes { get; init; }
    public required bool HasQtsDate { get; init; }
    public required bool HasEytsDate { get; init; }
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required string? StatedFirstName { get; init; }
    public required string? StatedMiddleName { get; init; }
    public required string? StatedLastName { get; init; }
    public required string? PreviousLastName { get; init; }
    public required DateOnly? DateOfBirth { get; init; }
    public required string? NationalInsuranceNumber { get; init; }
    public required string? EmailAddress { get; init; }
}
