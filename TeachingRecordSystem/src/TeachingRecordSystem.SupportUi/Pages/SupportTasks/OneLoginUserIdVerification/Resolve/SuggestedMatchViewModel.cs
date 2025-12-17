namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserIdVerification.Resolve;

public class SuggestedMatchViewModel
{
    public required char Identifier { get; init; }
    public required Guid PersonId { get; init; }
    public required string? Trn { get; init; }
    public required string? EmailAddress { get; init; }
    public required string? FirstName { get; init; }
    public required string? LastName { get; init; }
    public required DateOnly? DateOfBirth { get; init; }
    public required string? NationalInsuranceNumber { get; init; }
    public required IReadOnlyCollection<string>? PreviousNames { get; init; } = [];
    public required IReadOnlyCollection<PersonMatchedAttribute> MatchedAttributeTypes { get; init; }
}
