using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class Person
{
    public const int TrnExactLength = 7;
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
    public PersonStatus Status { get; set; }
    public Guid? MergedWithPersonId { get; set; }
    public Person? MergedWithPerson { get; }
    public required string? Trn { get; set; }
    public required string FirstName { get; set; }
    public required string MiddleName { get; set; }
    public required string LastName { get; set; }
    public required DateOnly? DateOfBirth { get; set; }  // A few DQT records in prod have a null DOB
    public string? EmailAddress { get; set; }
    public string? MobileNumber { get; set; }
    public string? NationalInsuranceNumber { get; set; }
    public Gender? Gender { get; set; }
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
    public QtlsStatus QtlsStatus { get; private set; }
    public DateOnly? EytsDate { get; internal set; }
    public bool HasEyps { get; internal set; }
    public DateOnly? PqtsDate { get; internal set; }
    public ICollection<Qualification>? Qualifications { get; protected set; }
    public ICollection<Alert>? Alerts { get; }
    public ICollection<PreviousName>? PreviousNames { get; }

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
        EmailAddress? emailAddress,
        MobileNumber? mobileNumber,
        NationalInsuranceNumber? nationalInsuranceNumber,
        Gender? gender,
        string? nameChangeReason,
        EventModels.File? nameChangeEvidenceFile,
        string? detailsChangeReason,
        string? detailsChangeReasonDetail,
        EventModels.File? detailsChangeEvidenceFile,
        EventModels.RaisedByUserInfo updatedBy,
        DateTime now,
        out PersonDetailsUpdatedEvent? @event)
    {
        var oldDetails = EventModels.PersonDetails.FromModel(this);

        FirstName = firstName;
        MiddleName = middleName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        MobileNumber = (string?)mobileNumber;
        EmailAddress = (string?)emailAddress;
        NationalInsuranceNumber = (string?)nationalInsuranceNumber;
        Gender = gender;
        UpdatedOn = now;

        var changes = PersonDetailsUpdatedEventChanges.None |
            (FirstName != oldDetails.FirstName ? PersonDetailsUpdatedEventChanges.FirstName : 0) |
            (MiddleName != oldDetails.MiddleName ? PersonDetailsUpdatedEventChanges.MiddleName : 0) |
            (LastName != oldDetails.LastName ? PersonDetailsUpdatedEventChanges.LastName : 0) |
            (DateOfBirth != oldDetails.DateOfBirth ? PersonDetailsUpdatedEventChanges.DateOfBirth : 0) |
            (EmailAddress != oldDetails.EmailAddress ? PersonDetailsUpdatedEventChanges.EmailAddress : 0) |
            (MobileNumber != oldDetails.MobileNumber ? PersonDetailsUpdatedEventChanges.MobileNumber : 0) |
            (NationalInsuranceNumber != oldDetails.NationalInsuranceNumber ? PersonDetailsUpdatedEventChanges.NationalInsuranceNumber : 0) |
            (Gender != oldDetails.Gender ? PersonDetailsUpdatedEventChanges.Gender : 0);

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
            NameChangeReason = nameChangeReason,
            NameChangeEvidenceFile = nameChangeEvidenceFile,
            DetailsChangeReason = detailsChangeReason,
            DetailsChangeReasonDetail = detailsChangeReasonDetail,
            DetailsChangeEvidenceFile = detailsChangeEvidenceFile,
            Changes = changes
        };
    }

    public void UpdateDetailsFromTrnRequest(
            DateOnly? dateOfBirth,
            EmailAddress? emailAddress,
            NationalInsuranceNumber? nationalInsuranceNumber,
            DateTime now)
    {
        DateOfBirth = dateOfBirth;
        EmailAddress = (string?)emailAddress;
        NationalInsuranceNumber = (string?)nationalInsuranceNumber;
        UpdatedOn = now;
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

    // This should be used for testing only
    public void UnsafeSetInductionStatus(
        InductionStatus status,
        InductionStatus statusWithoutExemption,
        DateOnly? startDate,
        DateOnly? completedDate,
        Guid[] exemptionReasonIds)
    {
        InductionStatus = status;
        InductionStatusWithoutExemption = statusWithoutExemption;
        InductionStartDate = startDate;
        InductionCompletedDate = completedDate;
        InductionExemptionReasonIds = exemptionReasonIds;
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

    public bool UnsafeRemoveInductionExemptionReason(
        Guid exemptionReasonId,
        EventModels.RaisedByUserInfo updatedBy,
        DateTime now)
    {
        if (!InductionExemptionReasonIds.Contains(exemptionReasonId))
        {
            return false;
        }

        InductionExemptionReasonIds = InductionExemptionReasonIds.Except([exemptionReasonId]).ToArray();
        InductionModifiedOn = now;

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

        var allExemptionReasonIds = GetAllInductionExemptionReasonIds();

        if (InductionStatus is InductionStatus.Exempt && (allExemptionReasonIds.Count == 0 && !InductionExemptWithoutReason))
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

    public bool RefreshInductionStatusForQtsProfessionalStatusChanged(
        DateTime now,
        IReadOnlyCollection<RouteToProfessionalStatusType> allRouteTypes,
        IEnumerable<RouteToProfessionalStatus>? routesHint = null)
    {
        var currentStatus = InductionStatus;
        var currentStatusWithoutExemption = InductionStatusWithoutExemption;

        var routes = routesHint ??
            Qualifications?
                .OfType<RouteToProfessionalStatus>()
                .Where(p => p.DeletedOn is null) ??
            throw new InvalidOperationException("Routes not loaded.");

        var holdsQtsProfessionalStatuses = routes
            .Where(p => allRouteTypes.Single(rt => rt.RouteToProfessionalStatusTypeId == p.RouteToProfessionalStatusTypeId).ProfessionalStatusType == ProfessionalStatusType.QualifiedTeacherStatus &&
                p.Status is RouteToProfessionalStatusStatus.Holds &&
                p.DeletedOn is null)
            .ToArray();

        var awardedBeforeInduction = holdsQtsProfessionalStatuses.Any(p => p.ExemptFromInductionDueToQtsDate == true);
        var requiredToComplete = holdsQtsProfessionalStatuses.Any() && !awardedBeforeInduction;

        if (!requiredToComplete && currentStatus is not InductionStatus.RequiredToComplete && currentStatus.RequiresQts())
        {
            throw new InvalidOperationException($"Cannot remove the induction requirement for a person who is '{InductionStatus}'.");
        }

        var newStatusWithoutExemption = requiredToComplete ?
            (InductionStatus.RequiredToComplete.IsHigherPriorityThan(currentStatusWithoutExemption) ? InductionStatus.RequiredToComplete : currentStatusWithoutExemption) :
            InductionStatus.None;

        var exempt = InductionExemptWithoutReason || InductionExemptionReasonIds.Any() || holdsQtsProfessionalStatuses.Any(r => r.ExemptFromInduction == true);
        var newStatus = exempt && InductionStatus.Exempt.IsHigherPriorityThan(newStatusWithoutExemption) ? InductionStatus.Exempt : newStatusWithoutExemption;

        bool changed = false;

        if (newStatus != currentStatus)
        {
            InductionStatus = newStatus;
            changed = true;
        }

        if (newStatusWithoutExemption != currentStatusWithoutExemption)
        {
            InductionStatusWithoutExemption = newStatusWithoutExemption;
            changed = true;
        }

        if (changed)
        {
            InductionModifiedOn = now;
        }

        return changed;
    }

    public bool RefreshProfessionalStatusAttributes(
        ProfessionalStatusType professionalStatusType,
        IReadOnlyCollection<RouteToProfessionalStatusType> allRouteTypes,
        IEnumerable<RouteToProfessionalStatus>? routesHint = null)
    {
        var routes = routesHint ??
            Qualifications?
                .OfType<RouteToProfessionalStatus>()
                .Where(p => p.DeletedOn is null) ??
            throw new InvalidOperationException("Routes not loaded.");

        var professionalStatusTypeByRouteId = allRouteTypes.ToDictionary(r => r.RouteToProfessionalStatusTypeId, r => r.ProfessionalStatusType);

        var holds = routes
            .Where(ps => professionalStatusTypeByRouteId[ps.RouteToProfessionalStatusTypeId] == professionalStatusType &&
                ps.Status is RouteToProfessionalStatusStatus.Holds)
            .ToArray();

        // We don't have awarded dates for EYPS
        if (professionalStatusType is ProfessionalStatusType.EarlyYearsProfessionalStatus)
        {
            var awarded = holds.Any();

            var changed = HasEyps != awarded;
            HasEyps = awarded;
            return changed;
        }

        Debug.Assert(holds.All(ps => ps.HoldsFrom is not null));
        var awardedDate = holds.Length > 0 ? holds.Min(ps => ps.HoldsFrom) : null;

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

    public bool RefreshQtlsStatus(IEnumerable<RouteToProfessionalStatus>? routesHint = null)
    {
        var routes = routesHint ??
            Qualifications?
                .OfType<RouteToProfessionalStatus>()
                .Where(p => p.DeletedOn is null) ??
            throw new InvalidOperationException("Routes not loaded.");

        var qtlsRoutes = routes
            .Where(r => r.RouteToProfessionalStatusTypeId == RouteToProfessionalStatusType.QtlsAndSetMembershipId &&
                r.Status is RouteToProfessionalStatusStatus.Holds &&
                r.DeletedOn is null)
            .ToArray();

        var currentStatus = QtlsStatus;

        if (qtlsRoutes.Length > 0 && currentStatus is not QtlsStatus.Active)
        {
            QtlsStatus = QtlsStatus.Active;
            return true;
        }

        if (qtlsRoutes.Length == 0 && currentStatus is QtlsStatus.Active)
        {
            QtlsStatus = QtlsStatus.Expired;
            return true;
        }

        return false;
    }

    public void UnsafeSetQtlsStatus(QtlsStatus qtlsStatus) => QtlsStatus = qtlsStatus;

    public static Person Create(
        string trn,
        string firstName,
        string middleName,
        string lastName,
        DateOnly? dateOfBirth,
        EmailAddress? emailAddress,
        MobileNumber? mobileNumber,
        NationalInsuranceNumber? nationalInsuranceNumber,
        Gender? gender,
        string? createReason,
        string? createReasonDetail,
        EventModels.File? evidenceFile,
        EventModels.RaisedByUserInfo updatedBy,
        DateTime now,
        out PersonCreatedEvent @event)
    {
        var person = new Person
        {
            PersonId = Guid.NewGuid(),
            Trn = trn,
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            MobileNumber = (string?)mobileNumber,
            EmailAddress = (string?)emailAddress,
            NationalInsuranceNumber = (string?)nationalInsuranceNumber,
            Gender = gender,
            CreatedOn = now,
            UpdatedOn = now,
        };

        @event = new PersonCreatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = now,
            RaisedBy = updatedBy,
            PersonId = person.PersonId,
            Details = EventModels.PersonDetails.FromModel(person),
            CreateReason = createReason,
            CreateReasonDetail = createReasonDetail,
            EvidenceFile = evidenceFile,
        };

        return person;
    }

    public static Person Create(
        string trn,
        string firstName,
        string middleName,
        string lastName,
        DateOnly? dateOfBirth,
        EmailAddress? emailAddress,
        NationalInsuranceNumber? nationalInsuranceNumber,
        DateTime now)
    {
        var person = new Person
        {
            PersonId = Guid.NewGuid(),
            Trn = trn,
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            CreatedOn = now,
            UpdatedOn = now
        };

        return person;
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

    public IReadOnlyCollection<Guid> GetAllInductionExemptionReasonIds(
        IEnumerable<RouteToProfessionalStatus>? routesHint = null)
    {
        var routes = routesHint ??
            Qualifications?
                .OfType<RouteToProfessionalStatus>()
                .Where(p => p.DeletedOn is null) ??
            throw new InvalidOperationException("Routes not loaded.");

        var holdsRoutes = routes
            .Where(s => s.Status is RouteToProfessionalStatusStatus.Holds)
            .ToArray();

        var routeLevelExemptionIds = holdsRoutes
            .Where(r => r.ExemptFromInduction == true)
            .Select(r => r.RouteToProfessionalStatusType!.InductionExemptionReasonId!.Value)
            .Concat(holdsRoutes
                .Where(r => r.ExemptFromInductionDueToQtsDate == true)
                .Select(_ => InductionExemptionReason.QualifiedBefore7May2000Id));

        return routeLevelExemptionIds.Concat(InductionExemptionReasonIds).Distinct().AsReadOnly();
    }
}
