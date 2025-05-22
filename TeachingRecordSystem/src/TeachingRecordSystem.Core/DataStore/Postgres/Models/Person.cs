using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class Person
{
    public const int FirstNameMaxLength = 100;
    public const int MiddleNameMaxLength = 100;
    public const int LastNameMaxLength = 100;
    public const int EmailAddressMaxLength = 100;
    public const int NationalInsuranceNumberMaxLength = 9;
    public const int MobileNumberMaxLength = 15;

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
    public string? MobileNumber { get; set; }
    public string? NationalInsuranceNumber { get; set; }
    public InductionStatus InductionStatus { get; private set; }
    public InductionStatus InductionStatusWithoutExemption { get; private set; }
    public Guid[] InductionExemptionReasonIds { get; private set; } = [];
    public bool InductionExemptWithoutReason { get; internal set; }  // internally set-able for testing
    public DateOnly? InductionStartDate { get; private set; }
    public DateOnly? InductionCompletedDate { get; private set; }
    public DateTime? InductionModifiedOn { get; private set; }
    public DateTime? CpdInductionModifiedOn { get; private set; }
    public DateTime? CpdInductionFirstModifiedOn { get; private set; }
    /// <summary>
    /// The timestamp given to us by CPD the last time they updated us.
    /// This is different to <see cref="CpdInductionModifiedOn"/> (which is our timestamp).
    /// </summary>
    public DateTime? CpdInductionCpdModifiedOn { get; private set; }
    public DateOnly? QtsDate { get; internal set; }
    public DateOnly? EytsDate { get; internal set; }
    public bool HasEyps { get; internal set; }
    public DateOnly? PqtsDate { get; internal set; }
    public ICollection<Qualification>? Qualifications { get; }
    public ICollection<Alert>? Alerts { get; }

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

    public void UpdateDetails(
        string firstName,
        string middleName,
        string lastName,
        DateOnly? dateOfBirth,
        string? emailAddress,
        string? mobileNumber,
        string? nationalInsuranceNumber,
        string changeReason,
        string changeReasonDetail,
        EventModels.File? evidenceFile,
        EventModels.RaisedByUserInfo updatedBy,
        DateTime now,
        out PersonDetailsUpdatedEvent? @event)
    {
        var oldDetails = EventModels.PersonDetails.FromModel(this);

        FirstName = firstName;
        MiddleName = middleName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        MobileNumber = mobileNumber;
        EmailAddress = emailAddress;
        NationalInsuranceNumber = nationalInsuranceNumber;

        var changes = PersonDetailsUpdatedEventChanges.None |
            (FirstName != oldDetails.FirstName ? PersonDetailsUpdatedEventChanges.FirstName : 0) |
            (MiddleName != oldDetails.MiddleName ? PersonDetailsUpdatedEventChanges.MiddleName : 0) |
            (LastName != oldDetails.LastName ? PersonDetailsUpdatedEventChanges.LastName : 0) |
            (DateOfBirth != oldDetails.DateOfBirth ? PersonDetailsUpdatedEventChanges.DateOfBirth : 0) |
            (EmailAddress != oldDetails.EmailAddress ? PersonDetailsUpdatedEventChanges.EmailAddress : 0) |
            (MobileNumber != oldDetails.MobileNumber ? PersonDetailsUpdatedEventChanges.MobileNumber : 0) |
            (NationalInsuranceNumber != oldDetails.NationalInsuranceNumber ? PersonDetailsUpdatedEventChanges.NationalInsuranceNumber : 0);

        if (changes == PersonDetailsUpdatedEventChanges.None)
        {
            @event = null;
            return;
        }

        @event = new PersonDetailsUpdatedEvent()
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = now,
            RaisedBy = updatedBy,
            PersonId = PersonId,
            Details = EventModels.PersonDetails.FromModel(this),
            OldDetails = oldDetails,
            ChangeReason = changeReason,
            ChangeReasonDetail = changeReasonDetail,
            EvidenceFile = evidenceFile,
            Changes = changes
        };
    }

    public void SetCpdInductionStatus(
        InductionStatus status,
        DateOnly? startDate,
        DateOnly? completedDate,
        DateTime cpdModifiedOn,
        EventModels.RaisedByUserInfo updatedBy,
        DateTime now,
        out PersonInductionUpdatedEvent? @event)
    {
        if (status is not (
            InductionStatus.RequiredToComplete
            or InductionStatus.InProgress
            or InductionStatus.Passed
            or InductionStatus.Failed))
        {
            throw new InvalidOperationException($"Cannot set the status to '{status}'.");
        }

        // FUTURE When we have QTS in TRS - assert person has QTS
        AssertInductionChangeIsValid(status, startDate, completedDate, exemptionReasonIds: []);

        var oldEventInduction = EventModels.Induction.FromModel(this);

        // Don't overwrite an Exempt status unless this status is higher priority
        if (InductionStatus is InductionStatus.Exempt && status.IsHigherPriorityThan(InductionStatus.Exempt))
        {
            InductionStatus = status;
        }
        else if (InductionStatus is not InductionStatus.Exempt)
        {
            InductionStatus = status;
        }

        InductionStatusWithoutExemption = status;
        InductionStartDate = startDate;
        InductionCompletedDate = completedDate;

        var changes = PersonInductionUpdatedEventChanges.None |
            (InductionStatus != oldEventInduction.Status ? PersonInductionUpdatedEventChanges.InductionStatus : 0) |
            (InductionStartDate != oldEventInduction.StartDate ? PersonInductionUpdatedEventChanges.InductionStartDate : 0) |
            (InductionCompletedDate != oldEventInduction.CompletedDate ? PersonInductionUpdatedEventChanges.InductionCompletedDate : 0) |
            (InductionStatusWithoutExemption != oldEventInduction.StatusWithoutExemption ? PersonInductionUpdatedEventChanges.InductionStatusWithoutExemption : 0);

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

        // If the status is being set to Exempt and the current status is lower priority than Exempt,
        // we need to 'remember' the current status so that if this exemption reason is removed we can revert to it.
        if (status is InductionStatus.Exempt)
        {
            if (InductionStatusWithoutExemption.IsHigherPriorityThan(InductionStatus.Exempt))
            {
                // e.g. If we're currently Complete but it's being overriden with Exempt,
                // our fallback for when this exemption is removed is RequiredToComplete.
                // FUTURE We should make this dynamic based on the presence of QTS.
                InductionStatusWithoutExemption = InductionStatus.RequiredToComplete;
                InductionStartDate = null;
                InductionCompletedDate = null;
            }

            InductionStatus = status;
            InductionExemptionReasonIds = exemptionReasonIds;
        }
        else
        {
            InductionStatus = status;
            InductionStatusWithoutExemption = status;
            InductionStartDate = startDate;
            InductionCompletedDate = completedDate;
            InductionExemptionReasonIds = [];
        }

        InductionExemptWithoutReason = false;

        var changes = PersonInductionUpdatedEventChanges.None |
            (InductionStatus != oldEventInduction.Status ? PersonInductionUpdatedEventChanges.InductionStatus : 0) |
            (InductionStatusWithoutExemption != oldEventInduction.StatusWithoutExemption ? PersonInductionUpdatedEventChanges.InductionStatusWithoutExemption : 0) |
            (InductionStartDate != oldEventInduction.StartDate ? PersonInductionUpdatedEventChanges.InductionStartDate : 0) |
            (InductionCompletedDate != oldEventInduction.CompletedDate ? PersonInductionUpdatedEventChanges.InductionCompletedDate : 0) |
            (!InductionExemptionReasonIds.ToHashSet().SetEquals(oldEventInduction.ExemptionReasonIds) ? PersonInductionUpdatedEventChanges.InductionExemptionReasons : 0) |
            (InductionExemptWithoutReason != oldEventInduction.InductionExemptWithoutReason ? PersonInductionUpdatedEventChanges.InductionExemptWithoutReason : 0);

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

        var changes = PersonInductionUpdatedEventChanges.InductionExemptionReasons;

        InductionExemptionReasonIds = InductionExemptionReasonIds.Concat([exemptionReasonId]).ToArray();
        InductionModifiedOn = now;

        if (InductionStatus.Exempt.IsHigherPriorityThan(InductionStatus))
        {
            InductionStatus = InductionStatus.Exempt;
            changes |= PersonInductionUpdatedEventChanges.InductionStatus;
        }

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

        var changes = PersonInductionUpdatedEventChanges.InductionExemptionReasons;

        InductionExemptionReasonIds = InductionExemptionReasonIds.Except([exemptionReasonId]).ToArray();
        InductionModifiedOn = now;

        if (InductionStatus is InductionStatus.Exempt && (InductionExemptionReasonIds.Length == 0 && !InductionExemptWithoutReason))
        {
            InductionStatus = InductionStatusWithoutExemption;
            changes |= PersonInductionUpdatedEventChanges.InductionStatus;
        }

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

    public bool TrySetWelshInductionStatus(
        bool passed,
        DateOnly? startDate,
        DateOnly? completedDate,
        EventModels.RaisedByUserInfo updatedBy,
        DateTime now,
        [NotNullWhen(true)] out PersonInductionUpdatedEvent? @event)
    {
        if (passed)
        {
            return AddInductionExemptionReason(InductionExemptionReason.PassedInWalesId, updatedBy, now, out @event);
        }

        var newStatus = InductionStatus.FailedInWales;

        if (InductionStatus.IsHigherPriorityThan(newStatus))
        {
            @event = null;
            return false;
        }

        var oldEventInduction = EventModels.Induction.FromModel(this);

        InductionStatus = newStatus;
        InductionStatusWithoutExemption = newStatus;
        InductionStartDate = startDate;
        InductionCompletedDate = completedDate;

        var changes = PersonInductionUpdatedEventChanges.None |
            (InductionStatus != oldEventInduction.Status ? PersonInductionUpdatedEventChanges.InductionStatus : 0) |
            (InductionStatusWithoutExemption != oldEventInduction.StatusWithoutExemption ? PersonInductionUpdatedEventChanges.InductionStatusWithoutExemption : 0) |
            (InductionStartDate != oldEventInduction.StartDate ? PersonInductionUpdatedEventChanges.InductionStartDate : 0) |
            (InductionCompletedDate != oldEventInduction.CompletedDate ? PersonInductionUpdatedEventChanges.InductionCompletedDate : 0);

        if (changes == PersonInductionUpdatedEventChanges.None)
        {
            @event = null;
            return false;
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
        return (CpdInductionModifiedOn is not null && (InductionCompletedDate is null || InductionCompletedDate > sevenYearsAgo));
    }

    public bool RefreshProfessionalStatusAttributes(
        ProfessionalStatusType professionalStatusType,
        IReadOnlyCollection<RouteToProfessionalStatus> allRoutes,
        IEnumerable<ProfessionalStatus>? professionalStatusesHint = null)
    {
        var professionalStatuses = professionalStatusesHint ??
            Qualifications?
                .OfType<ProfessionalStatus>()?
                .Where(p => p.DeletedOn is null) ??
            throw new InvalidOperationException("No professional statuses.");

        var professionalStatusTypeByRouteId = allRoutes.ToDictionary(r => r.RouteToProfessionalStatusId, r => r.ProfessionalStatusType);

        var awardedOrApproved = professionalStatuses
            .Where(ps => professionalStatusTypeByRouteId[ps.RouteToProfessionalStatusId] == professionalStatusType &&
                ps.Status is ProfessionalStatusStatus.Approved or ProfessionalStatusStatus.Awarded)
            .ToArray();

        // We don't have awarded dates for EYPS
        if (professionalStatusType is ProfessionalStatusType.EarlyYearsProfessionalStatus)
        {
            var awarded = awardedOrApproved.Any();

            var changed = HasEyps != awarded;
            HasEyps = awarded;
            return changed;
        }

        Debug.Assert(awardedOrApproved.All(ps => ps.AwardedDate is not null));
        var awardedDate = awardedOrApproved.Length > 0 ? awardedOrApproved.Min(ps => ps.AwardedDate) : null;

        if (professionalStatusType is ProfessionalStatusType.QualifiedTeacherStatus)
        {
            var changed = QtsDate != awardedDate;
            QtsDate = awardedDate;
            return changed;
        }
        else if (professionalStatusType is ProfessionalStatusType.EarlyYearsTeacherStatus)
        {
            var changed = EytsDate != awardedDate;
            EytsDate = awardedDate;
            return changed;
        }
        else
        {
            Debug.Assert(professionalStatusType is ProfessionalStatusType.PartialQualifiedTeacherStatus);
            var changed = PqtsDate != awardedDate;
            PqtsDate = awardedDate;
            return changed;
        }
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
