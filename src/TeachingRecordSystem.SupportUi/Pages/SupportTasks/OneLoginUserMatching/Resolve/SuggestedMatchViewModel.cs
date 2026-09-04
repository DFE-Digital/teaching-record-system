namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching.Resolve;

public class SuggestedMatchViewModel
{
    public required char Identifier { get; init; }
    public required Guid PersonId { get; init; }
    public required string Trn { get; init; }
    public required string? EmailAddress { get; init; }
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly? DateOfBirth { get; init; }
    public required string? NationalInsuranceNumber { get; init; }
    public required IReadOnlyCollection<string> PreviousNames { get; init; }
    public required IReadOnlyCollection<PersonMatchedAttribute> MatchedAttributeTypes { get; init; }
    public required SuggestedMatchProfessionalStatusDetailsViewModel? QtlsDetails { get; init; }
    public required IReadOnlyCollection<SuggestedMatchProfessionalStatusDetailsViewModel> QtsDetails { get; init; }

    public bool HasNameMismatch => !(MatchedAttributeTypes.Contains(PersonMatchedAttribute.FirstName) && MatchedAttributeTypes.Contains(PersonMatchedAttribute.LastName));
}

public sealed record SuggestedMatchProfessionalStatusDetailsViewModel
{
    public required string Heading { get; init; }
    public required string? YearReceived { get; init; }
    public required string? Provider { get; init; }
    public required IReadOnlyCollection<string> Subjects { get; init; }

    public bool HasAnyDetails =>
        !string.IsNullOrWhiteSpace(YearReceived) ||
        Provider is not null ||
        Subjects.Count > 0;
}
