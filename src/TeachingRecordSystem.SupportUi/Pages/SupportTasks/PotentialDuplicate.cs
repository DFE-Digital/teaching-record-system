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
}

public static class PotentialDuplicateExtensions
{
    public static bool HasAnyNamePartMismatch(this PotentialDuplicate potentialDuplicate, string? requestFirstName, string? requestMiddleName, string? requestLastName)
    {
        return IsFieldMismatch(potentialDuplicate.FirstName, requestFirstName, potentialDuplicate.MatchedAttributes.Contains(PersonMatchedAttribute.FirstName))
            || IsFieldMismatch(potentialDuplicate.MiddleName, requestMiddleName, potentialDuplicate.MatchedAttributes.Contains(PersonMatchedAttribute.MiddleName))
            || IsFieldMismatch(potentialDuplicate.LastName, requestLastName, potentialDuplicate.MatchedAttributes.Contains(PersonMatchedAttribute.LastName));
    }

    private static bool IsFieldMismatch(string? potentialDuplicateValue, string? requestValue, bool isMatched)
    {
        // Both values are null/empty - not a mismatch
        if (string.IsNullOrEmpty(potentialDuplicateValue) && string.IsNullOrEmpty(requestValue))
        {
            return false;
        }

        // At least one has a value - mismatch if not in matched attributes
        return !isMatched;
    }
}
