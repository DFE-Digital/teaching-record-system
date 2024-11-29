using System.Diagnostics;
using System.Runtime.CompilerServices;
using Optional;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class Person
{
    public required Guid PersonId { get; init; }
    public required DateTime? CreatedOn { get; init; }
    public required DateTime? UpdatedOn { get; set; }
    public DateTime? DeletedOn { get; set; }
    public required string? Trn { get; set; }
    public required string FirstName { get; set; }
    public required string MiddleName { get; set; }
    public required string LastName { get; set; }
    public required DateOnly? DateOfBirth { get; set; }  // A few DQT records in prod have a null DOB
    public string? EmailAddress { get; set; }
    public string? NationalInsuranceNumber { get; set; }
    public InductionStatus InductionStatus { get; private set; }
    public InductionExemptionReasons InductionExemptionReasons { get; private set; }
    public DateOnly? InductionStartDate { get; private set; }
    public DateOnly? InductionCompletedDate { get; private set; }
    public DateTime? InductionModifiedOn { get; private set; }
    public InductionStatus? CpdInductionStatus { get; private set; }
    public DateOnly? CpdInductionStartDate { get; private set; }
    public DateOnly? CpdInductionCompletedDate { get; private set; }
    public DateTime? CpdInductionModifiedOn { get; private set; }
    public DateTime? CpdInductionFirstModifiedOn { get; private set; }
    public DateTime? CpdInductionCpdModifiedOn { get; private set; }
    public ICollection<Qualification> Qualifications { get; } = new List<Qualification>();
    public ICollection<Alert> Alerts { get; } = new List<Alert>();

    public Guid? DqtContactId { get; init; }
    public DateTime? DqtFirstSync { get; set; }
    public DateTime? DqtLastSync { get; set; }
    public int? DqtState { get; set; }
    public DateTime? DqtCreatedOn { get; set; }
    public DateTime? DqtModifiedOn { get; set; }
    public string? DqtFirstName { get; set; }
    public string? DqtMiddleName { get; set; }
    public string? DqtLastName { get; set; }
    public DateTime? DqtInductionLastSync { get; set; }
    public DateTime? DqtInductionModifiedOn { get; set; }

    public void SetCpdInductionStatus(
        InductionStatus status,
        DateOnly? startDate,
        DateOnly? completedDate,
        DateTime cpdModifiedOn,
        EventModels.RaisedByUserInfo updatedBy,
        DateTime now,
        out PersonInductionUpdatedEvent? @event)
    {
        if (status == CpdInductionStatus &&
            startDate == CpdInductionStartDate &&
            completedDate == CpdInductionCompletedDate)
        {
            @event = null;
            return;
        }

        // FUTURE When we have QTS in TRS - assert person has QTS
        AssertInductionChangeIsValid(status, startDate, completedDate, exemptionReason: null);

        // If the CPD status is not Passed and we know the person is Exempt then the overall status is Exempt.
        // Otherwise, the overall status is set to match the CPD status.
        // It's important we never overwrite InductionExemptionReasons here since we're using it to remember
        // if somebody is exempt. In future we may be able to get this from the {route to} professional status instead.

        var isExempt = InductionExemptionReasons != InductionExemptionReasons.None;
        var (newOverallStatus, newOverallStartDate, newOverallCompletedDate) = isExempt && status != InductionStatus.Passed
            ? (InductionStatus.Exempt, null, null)
            : (status, startDate, completedDate);

        var changes = PersonInductionUpdatedEventChanges.None |
            (InductionStatus != newOverallStatus ? PersonInductionUpdatedEventChanges.InductionStatus : 0) |
            (InductionStartDate != newOverallStartDate ? PersonInductionUpdatedEventChanges.InductionStartDate : 0) |
            (InductionCompletedDate != newOverallCompletedDate ? PersonInductionUpdatedEventChanges.InductionCompletedDate : 0) |
            (CpdInductionStatus != status ? PersonInductionUpdatedEventChanges.CpdInductionStatus : 0) |
            (CpdInductionStartDate != startDate ? PersonInductionUpdatedEventChanges.CpdInductionStartDate : 0) |
            (CpdInductionCompletedDate != completedDate ? PersonInductionUpdatedEventChanges.CpdInductionCompletedDate : 0);

        if (changes == PersonInductionUpdatedEventChanges.None)
        {
            @event = null;
            return;
        }

        InductionStatus = newOverallStatus;
        InductionStartDate = newOverallStartDate;
        InductionCompletedDate = newOverallCompletedDate;
        CpdInductionStatus = status;
        CpdInductionStartDate = startDate;
        CpdInductionCompletedDate = completedDate;
        CpdInductionCpdModifiedOn = cpdModifiedOn;
        CpdInductionModifiedOn = now;
        CpdInductionFirstModifiedOn ??= now;

        if ((changes & (
             PersonInductionUpdatedEventChanges.InductionStatus |
             PersonInductionUpdatedEventChanges.InductionStartDate |
             PersonInductionUpdatedEventChanges.InductionCompletedDate))
            != 0)
        {
            InductionModifiedOn = now;
        }

        @event = new PersonInductionUpdatedEvent()
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = now,
            RaisedBy = updatedBy,
            PersonId = PersonId,
            InductionStatus = newOverallStatus,
            InductionStartDate = newOverallStartDate,
            InductionCompletedDate = newOverallCompletedDate,
            InductionExemptionReasons = InductionExemptionReasons,
            CpdInductionStatus = Option.Some(status),
            CpdInductionStartDate = Option.Some(startDate),
            CpdInductionCompletedDate = Option.Some(completedDate),
            CpdInductionCpdModifiedOn = Option.Some(cpdModifiedOn),
            ChangeReason = null,
            ChangeReasonDetail = null,
            EvidenceFile = null,
            Changes = changes
        };
    }

    public void SetInductionStatus(
        InductionStatus status,
        DateOnly? startDate,
        DateOnly? completedDate,
        InductionExemptionReasons exemptionReasons,
        EventModels.RaisedByUserInfo updatedBy,
        DateTime now,
        out PersonInductionUpdatedEvent? @event)
    {
        // FUTURE When we have QTS in TRS - assert person has QTS
        AssertInductionChangeIsValid(status, startDate, completedDate, exemptionReasons);

        var changes = PersonInductionUpdatedEventChanges.None |
            (InductionStatus != status ? PersonInductionUpdatedEventChanges.InductionStatus : 0) |
            (InductionStartDate != startDate ? PersonInductionUpdatedEventChanges.InductionStartDate : 0) |
            (InductionCompletedDate != completedDate ? PersonInductionUpdatedEventChanges.InductionCompletedDate : 0) |
            (InductionExemptionReasons != exemptionReasons ? PersonInductionUpdatedEventChanges.InductionExemptionReasons : 0);

        if (changes == PersonInductionUpdatedEventChanges.None)
        {
            @event = null;
            return;
        }

        InductionStatus = status;
        InductionStartDate = startDate;
        InductionCompletedDate = completedDate;
        InductionExemptionReasons = exemptionReasons;
        InductionModifiedOn = now;

        @event = new PersonInductionUpdatedEvent()
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = now,
            RaisedBy = updatedBy,
            PersonId = PersonId,
            InductionStatus = status,
            InductionStartDate = startDate,
            InductionCompletedDate = completedDate,
            InductionExemptionReasons = InductionExemptionReasons,
            CpdInductionStatus = default,
            CpdInductionStartDate = default,
            CpdInductionCompletedDate = default,
            CpdInductionCpdModifiedOn = default,
            ChangeReason = null,
            ChangeReasonDetail = null,
            EvidenceFile = null,
            Changes = changes
        };
    }

    private static void AssertInductionChangeIsValid(
        InductionStatus status,
        DateOnly? startDate,
        DateOnly? completedDate,
        InductionExemptionReasons? exemptionReason)
    {
        if (status is InductionStatus.None or InductionStatus.RequiredToComplete)
        {
            EnsureArgumentIsNullForInduction(startDate);
            EnsureArgumentIsNullForInduction(completedDate);
            EnsureArgumentIsNullForInduction(exemptionReason);
        }
        else if (status is InductionStatus.Exempt)
        {
            EnsureArgumentIsNullForInduction(startDate);
            EnsureArgumentIsNullForInduction(completedDate);
            EnsureArgumentIsNotNullForInduction(exemptionReason);
        }
        else if (status is InductionStatus.InProgress)
        {
            EnsureArgumentIsNotNullForInduction(startDate);
            EnsureArgumentIsNullForInduction(completedDate);
            EnsureArgumentIsNullForInduction(exemptionReason);
        }
        else if (status is InductionStatus.Passed)
        {
            EnsureArgumentIsNotNullForInduction(startDate);
            EnsureArgumentIsNotNullForInduction(completedDate);
            EnsureArgumentIsNullForInduction(exemptionReason);
        }
        else if (status is InductionStatus.FailedInWales)
        {
            EnsureArgumentIsNullForInduction(startDate);
            EnsureArgumentIsNullForInduction(completedDate);
            EnsureArgumentIsNullForInduction(exemptionReason);
        }
        else
        {
            throw new ArgumentException($"Unknown status: '{status}'.", nameof(status));
        }

        void EnsureArgumentIsNullForInduction(object? arg, [CallerArgumentExpression(nameof(arg))] string? paramName = "") =>
            Debug.Assert(arg is null, $"{paramName} must be null when the status is '{status}'.");

        void EnsureArgumentIsNotNullForInduction(object? arg, [CallerArgumentExpression(nameof(arg))] string? paramName = "") =>
            Debug.Assert(arg is not null, $"{paramName} cannot be null when the status is '{status}'.");
    }
}
