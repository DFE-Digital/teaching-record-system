using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Optional.Unsafe;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class Person
{
    public static DateOnly EarliestInductionStartDate => new(1999, 5, 7);

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

    public InductionStatus InductionStatus
    {
        get => GetInduction().Status;
        private set => _ = value;  // EF requires a setter but it's effectively not used
    }

    public DateTime? InductionModifiedOn { get; private set; }
    public InductionStatus? CpdInductionStatus { get; private set; }
    public DateTime? CpdInductionModifiedOn { get; private set; }
    public DateTime? CpdInductionFirstModifiedOn { get; private set; }
    public bool InductionRequiredToComplete { get; private set; }
    public bool InductionPassed { get; private set; }
    public bool InductionFailed { get; private set; }
    public Guid[] InductionExemptionReasonIds { get; private set; } = [];
    internal DateOnly? InductionStartDate { get; private set; }
    internal DateOnly? InductionCompletedDate { get; private set; }
    internal DateOnly? InductionFailedInWalesStartDate { get; private set; }
    internal DateOnly? InductionFailedInWalesCompletedDate { get; private set; }
    /// <summary>
    /// The timestamp given to us by CPD the last time they updated us.
    /// This is different to <see cref="CpdInductionModifiedOn"/> (which is our timestamp).
    /// </summary>
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

    public Induction GetInduction() => this switch
    {
        { InductionFailed: true } => new Induction(InductionStatus.Failed, InductionStartDate, InductionCompletedDate, []),
        { InductionPassed: true } => new Induction(InductionStatus.Passed, InductionStartDate, InductionCompletedDate, []),
        { InductionExemptionReasonIds: not [] } => new Induction(InductionStatus.Exempt, null, null, InductionExemptionReasonIds),
        { InductionStartDate: not null } => new Induction(InductionStatus.InProgress, InductionStartDate, null, []),
        { InductionFailedInWalesStartDate: not null } => new Induction(InductionStatus.FailedInWales, InductionFailedInWalesStartDate, InductionFailedInWalesCompletedDate, []),
        { InductionRequiredToComplete: true } => new Induction(InductionStatus.RequiredToComplete, null, null, []),
        _ => new Induction(InductionStatus.None, null, null, [])
    };

    public void SetCpdInductionStatus(
        InductionStatus status,
        DateOnly? startDate,
        DateOnly? completedDate,
        DateTime cpdModifiedOn,
        EventModels.RaisedByUserInfo updatedBy,
        DateTime now,
        out PersonInductionUpdatedEvent? @event)
    {
        // FUTURE When we have QTS in TRS - assert person has QTS
        AssertInductionChangeIsValid(status, startDate, completedDate, exemptionReasonIds: []);

        var oldEventInduction = EventModels.Induction.FromModel(this);

        InductionRequiredToComplete = status != InductionStatus.None;
        InductionPassed = status == InductionStatus.Passed;
        InductionFailed = status == InductionStatus.Failed;
        InductionStartDate = startDate;
        InductionCompletedDate = completedDate;
        CpdInductionStatus = status;

        var changes = PersonInductionUpdatedEventChanges.None |
            (InductionStatus != oldEventInduction.Status ? PersonInductionUpdatedEventChanges.Status : 0) |
            (CpdInductionStatus != oldEventInduction.CpdStatus.ToNullable() ? PersonInductionUpdatedEventChanges.CpdStatus : 0) |
            (InductionRequiredToComplete != oldEventInduction.RequiredToComplete ? PersonInductionUpdatedEventChanges.RequiredToComplete : 0) |
            (InductionPassed != oldEventInduction.Passed ? PersonInductionUpdatedEventChanges.Passed : 0) |
            (InductionFailed != oldEventInduction.Failed ? PersonInductionUpdatedEventChanges.Failed : 0) |
            (InductionStartDate != oldEventInduction.StartDate ? PersonInductionUpdatedEventChanges.StartDate : 0) |
            (InductionCompletedDate != oldEventInduction.CompletedDate ? PersonInductionUpdatedEventChanges.CompletedDate : 0);

        if (changes == PersonInductionUpdatedEventChanges.None)
        {
            @event = null;
            return;
        }

        InductionModifiedOn = now;
        CpdInductionFirstModifiedOn ??= now;
        CpdInductionModifiedOn = now;
        CpdInductionCpdModifiedOn = cpdModifiedOn;

        @event = new PersonInductionUpdatedEvent()
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = now,
            RaisedBy = updatedBy,
            PersonId = PersonId,
            Induction = EventModels.Induction.FromModel(this),
            OldInduction = oldEventInduction,
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
        Guid[] exemptionReasonIds,
        string? changeReason,
        string? changeReasonDetail,
        EventModels.File? evidenceFile,
        EventModels.RaisedByUserInfo updatedBy,
        DateTime now,
        out PersonInductionUpdatedEvent? @event)
    {
        // N.B. We allow missing data fields as some migrated data has missing fields
        // and we want to be able to test such scenarios.

        var oldEventInduction = EventModels.Induction.FromModel(this);

        InductionRequiredToComplete = status != InductionStatus.None;
        InductionPassed = status == InductionStatus.Passed;
        InductionFailed = status == InductionStatus.Failed;
        InductionStartDate = status != InductionStatus.FailedInWales ? startDate : null;
        InductionCompletedDate = status != InductionStatus.FailedInWales ? completedDate : null;
        InductionFailedInWalesStartDate = status == InductionStatus.FailedInWales ? startDate : null;
        InductionFailedInWalesCompletedDate = status == InductionStatus.FailedInWales ? completedDate : null;
        InductionExemptionReasonIds = exemptionReasonIds;

        Debug.Assert(InductionStatus == status);

        var changes = PersonInductionUpdatedEventChanges.None |
            (InductionStatus != oldEventInduction.Status ? PersonInductionUpdatedEventChanges.Status : 0) |
            (InductionRequiredToComplete != oldEventInduction.RequiredToComplete ? PersonInductionUpdatedEventChanges.RequiredToComplete : 0) |
            (InductionPassed != oldEventInduction.Passed ? PersonInductionUpdatedEventChanges.Passed : 0) |
            (InductionFailed != oldEventInduction.Failed ? PersonInductionUpdatedEventChanges.Failed : 0) |
            (InductionStartDate != oldEventInduction.StartDate ? PersonInductionUpdatedEventChanges.StartDate : 0) |
            (InductionCompletedDate != oldEventInduction.CompletedDate ? PersonInductionUpdatedEventChanges.CompletedDate : 0) |
            (InductionExemptionReasonIds != oldEventInduction.ExemptionReasonIds ? PersonInductionUpdatedEventChanges.ExemptionReasons : 0) |
            (InductionFailedInWalesStartDate != oldEventInduction.FailedInWalesStartDate ? PersonInductionUpdatedEventChanges.FailedInWalesStartDate : 0) |
            (InductionFailedInWalesCompletedDate != oldEventInduction.FailedInWalesCompletedDate ? PersonInductionUpdatedEventChanges.FailedInWalesCompletedDate : 0);

        if (changes == PersonInductionUpdatedEventChanges.None)
        {
            @event = null;
            return;
        }

        InductionModifiedOn = now;

        @event = new PersonInductionUpdatedEvent()
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = now,
            RaisedBy = updatedBy,
            PersonId = PersonId,
            Induction = EventModels.Induction.FromModel(this),
            OldInduction = oldEventInduction,
            ChangeReason = changeReason,
            ChangeReasonDetail = changeReasonDetail,
            EvidenceFile = evidenceFile,
            Changes = changes
        };
    }

    public void SetWelshInductionStatus(
        bool passed,
        DateOnly? startDate,
        DateOnly? completedDate,
        EventModels.RaisedByUserInfo updatedBy,
        DateTime now,
        out PersonInductionUpdatedEvent? @event)
    {
        var oldEventInduction = EventModels.Induction.FromModel(this);
        var changes = PersonInductionUpdatedEventChanges.None;

        if (passed)
        {
            changes |= !InductionExemptionReasonIds.Contains(InductionExemptionReason.PassedInWalesId)
                ? PersonInductionUpdatedEventChanges.ExemptionReasons
                : PersonInductionUpdatedEventChanges.None;

            if (changes == PersonInductionUpdatedEventChanges.None)
            {
                @event = default;
                return;
            }

            InductionExemptionReasonIds = InductionExemptionReasonIds
                .Append(InductionExemptionReason.PassedInWalesId)
                .ToArray();
        }
        else
        {
            changes |= (InductionFailedInWalesStartDate != startDate ? PersonInductionUpdatedEventChanges.FailedInWalesStartDate : 0) |
                (InductionFailedInWalesCompletedDate != completedDate ? PersonInductionUpdatedEventChanges.FailedInWalesCompletedDate : 0);

            if (changes == PersonInductionUpdatedEventChanges.None)
            {
                @event = default;
                return;
            }

            InductionFailedInWalesStartDate = startDate;
            InductionFailedInWalesCompletedDate = completedDate;
        }

        InductionModifiedOn = now;

        changes |= InductionStatus != oldEventInduction.Status ? PersonInductionUpdatedEventChanges.Status : 0;

        @event = new PersonInductionUpdatedEvent()
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = now,
            RaisedBy = updatedBy,
            PersonId = PersonId,
            Induction = EventModels.Induction.FromModel(this),
            OldInduction = oldEventInduction,
            ChangeReason = null,
            ChangeReasonDetail = null,
            EvidenceFile = null,
            Changes = changes
        };
    }

    public bool AddInductionExemptionReason(
        Guid exemptionReasonId,
        EventModels.RaisedByUserInfo updatedBy,
        DateTime now,
        [NotNullWhen(true)] out PersonInductionUpdatedEvent? @event)
    {
        if (InductionExemptionReasonIds.Contains(exemptionReasonId))
        {
            @event = null;
            return false;
        }

        var oldEventInduction = EventModels.Induction.FromModel(this);

        InductionExemptionReasonIds = InductionExemptionReasonIds.Concat([exemptionReasonId]).ToArray();
        InductionModifiedOn = now;

        var changes = PersonInductionUpdatedEventChanges.ExemptionReasons |
            (InductionStatus != oldEventInduction.Status ? PersonInductionUpdatedEventChanges.Status : 0);

        @event = new PersonInductionUpdatedEvent()
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = now,
            RaisedBy = updatedBy,
            PersonId = PersonId,
            Induction = EventModels.Induction.FromModel(this),
            OldInduction = oldEventInduction,
            ChangeReason = null,
            ChangeReasonDetail = null,
            EvidenceFile = null,
            Changes = changes
        };

        return true;
    }

    public bool RemoveInductionExemptionReason(
        Guid exemptionReasonId,
        EventModels.RaisedByUserInfo updatedBy,
        DateTime now,
        [NotNullWhen(true)] out PersonInductionUpdatedEvent? @event)
    {
        if (!InductionExemptionReasonIds.Contains(exemptionReasonId))
        {
            @event = null;
            return false;
        }

        var oldEventInduction = EventModels.Induction.FromModel(this);

        InductionExemptionReasonIds = InductionExemptionReasonIds.Except([exemptionReasonId]).ToArray();
        InductionModifiedOn = now;

        var changes = PersonInductionUpdatedEventChanges.ExemptionReasons |
            (InductionStatus != oldEventInduction.Status ? PersonInductionUpdatedEventChanges.Status : 0);

        @event = new PersonInductionUpdatedEvent()
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = now,
            RaisedBy = updatedBy,
            PersonId = PersonId,
            Induction = EventModels.Induction.FromModel(this),
            OldInduction = oldEventInduction,
            ChangeReason = null,
            ChangeReasonDetail = null,
            EvidenceFile = null,
            Changes = changes
        };

        return true;
    }

    public static bool ValidateInductionData(
        InductionStatus status,
        DateOnly? startDate,
        DateOnly? completedDate,
        Guid[] exemptionReasonIds,
        [NotNullWhen(false)] out string? error)
    {
        var requiresStartDate = status.RequiresStartDate();
        var requiresCompletedDate = status.RequiresCompletedDate();
        var requiresExemptionReason = status.RequiresExemptionReasons();

        if (requiresStartDate && startDate is null)
        {
            error = $"Start date cannot be null when the status is: '{status}'.";
            return false;
        }

        if (!requiresStartDate && startDate is not null)
        {
            error = $"Start date must be null when the status is: '{status}'.";
            return false;
        }

        if (requiresCompletedDate && completedDate is null)
        {
            error = $"Completed date cannot be null when the status is: '{status}'.";
            return false;
        }

        if (!requiresCompletedDate && completedDate is not null)
        {
            error = $"Completed date must be null when the status is: '{status}'.";
            return false;
        }

        if (requiresExemptionReason && !exemptionReasonIds.Any())
        {
            error = $"Exemption reasons cannot be empty when the status is: '{status}'.";
            return false;
        }

        if (!requiresExemptionReason && exemptionReasonIds.Any())
        {
            error = $"Exemption reasons must be empty when the status is: '{status}'.";
            return false;
        }

        error = null;
        return true;
    }

    public bool InductionStatusManagedByCpd(DateOnly now)
    {
        var sevenYearsAgo = now.AddYears(-7);
        return CpdInductionStatus is not null && (InductionCompletedDate is null || InductionCompletedDate > sevenYearsAgo);
    }

    private static void AssertInductionChangeIsValid(
        InductionStatus status,
        DateOnly? startDate,
        DateOnly? completedDate,
        Guid[] exemptionReasonIds)
    {
        if (!ValidateInductionData(status, startDate, completedDate, exemptionReasonIds, out var error))
        {
            Debug.Fail(error);
        }
    }
}

public sealed record Induction(InductionStatus Status, DateOnly? StartDate, DateOnly? CompletedDate, Guid[] ExemptionReasonIds);
