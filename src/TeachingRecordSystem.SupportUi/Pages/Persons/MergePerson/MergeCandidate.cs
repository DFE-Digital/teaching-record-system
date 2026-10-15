using TeachingRecordSystem.Core.Events.Models;
using TeachingRecordSystem.Core.Services.Persons;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.MergePerson;

public record MergeCandidate
{
    public required char Identifier { get; init; }
    public required string Trn { get; init; }
    public required Guid PersonId { get; init; }
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly? DateOfBirth { get; init; }
    public required string? EmailAddress { get; init; }
    public required string? NationalInsuranceNumber { get; init; }
    public required Gender? Gender { get; init; }
    public required PersonStatus Status { get; init; }
    public required InductionStatus InductionStatus { get; init; }
    public required InductionStatus InductionStatusWithoutExemption { get; init; }
    public required DateOnly? InductionStartDate { get; init; }
    public required DateOnly? InductionCompletedDate { get; init; }
    public required int ActiveAlertCount { get; init; }
    public required PersonDetails Attributes { get; init; }
    public required IReadOnlyCollection<PersonMatchedAttribute> MatchedAttributes { get; init; }

    public PersonMergeDetails MergeDetails =>
        new(Status, InductionStatus, InductionStatusWithoutExemption, InductionStartDate, InductionCompletedDate, HasOpenAlert: ActiveAlertCount > 0);
}
