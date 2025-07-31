using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.ManualMerge;

public record PotentialDuplicate
{
    private static readonly InductionStatus[] _invalidInductionStatusesForMerge = [InductionStatus.InProgress, InductionStatus.Passed, InductionStatus.Failed];

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
    public required int ActiveAlertCount { get; init; }
    public required PersonAttributes Attributes { get; init; }
    public required IReadOnlyCollection<PersonMatchedAttribute> MatchedAttributes { get; init; }

    public bool HasActiveAlerts => ActiveAlertCount > 0;
    public bool HasBeenDeactivated => Status == PersonStatus.Deactivated;
    public bool HasInvalidInductionStatus => _invalidInductionStatusesForMerge.Contains(InductionStatus);
    public bool IsInvalid => HasBeenDeactivated || HasInvalidInductionStatus || HasActiveAlerts;
}
