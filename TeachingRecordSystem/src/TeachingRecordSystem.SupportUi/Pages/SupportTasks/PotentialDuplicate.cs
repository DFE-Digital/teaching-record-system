namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks;

public record PotentialDuplicate
{
    public required char Identifier { get; init; }
    public required Guid PersonId { get; init; }
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly? DateOfBirth { get; init; }
    public required string? EmailAddress { get; init; }
    public required string? NationalInsuranceNumber { get; init; }
    public required string Trn { get; init; }
    public required Gender? Gender { get; init; }
    public required bool HasQts { get; init; }
    public required bool HasEyts { get; init; }
    public required bool HasActiveAlerts { get; init; }
    public required IReadOnlyCollection<string> PreviousNames { get; init; }
    public required IReadOnlyCollection<PersonMatchedAttribute> MatchedAttributes { get; init; }

    public bool HasFirstNameMismatch => !(String.IsNullOrEmpty(FirstName) || MatchedAttributes.Contains(PersonMatchedAttribute.FirstName));
    public bool HasMiddleNameMismatch => !(String.IsNullOrEmpty(MiddleName) || MatchedAttributes.Contains(PersonMatchedAttribute.MiddleName));
    public bool HasLastNameMismatch => !(String.IsNullOrEmpty(LastName) || MatchedAttributes.Contains(PersonMatchedAttribute.LastName));
}
