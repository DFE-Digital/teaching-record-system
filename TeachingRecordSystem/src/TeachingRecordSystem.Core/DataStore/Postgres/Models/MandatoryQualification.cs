namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class MandatoryQualification : Qualification
{
    public MandatoryQualificationProvider? Provider { get; }
    public required Guid? ProviderId { get; set; }
    public required MandatoryQualificationSpecialism? Specialism { get; set; }
    public required MandatoryQualificationStatus? Status { get; set; }
    public required DateOnly? StartDate { get; set; }
    public required DateOnly? EndDate { get; set; }

    public Guid? DqtMqEstablishmentId { get; set; }
    public Guid? DqtSpecialismId { get; set; }

    public static MandatoryQualification Create(
        Guid personId,
        Guid providerId,
        MandatoryQualificationSpecialism specialism,
        MandatoryQualificationStatus status,
        DateOnly startDate,
        DateOnly? endDate,
        EventModels.RaisedByUserInfo createdBy,
        DateTime now,
        out MandatoryQualificationCreatedEvent @event)
    {
        var qualificationId = Guid.NewGuid();

        var qualification = new MandatoryQualification()
        {
            QualificationId = qualificationId,
            CreatedOn = now,
            UpdatedOn = now,
            PersonId = personId,
            ProviderId = providerId,
            Status = status,
            Specialism = specialism,
            StartDate = startDate,
            EndDate = endDate
        };

        @event = new MandatoryQualificationCreatedEvent()
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = now,
            RaisedBy = createdBy,
            PersonId = personId,
            MandatoryQualification = EventModels.MandatoryQualification.FromModel(
                qualification,
                providerNameHint: MandatoryQualificationProvider.GetById(providerId).Name)
        };

        return qualification;
    }

    public void Delete(
        string? deletionReason,
        string? deletionReasonDetail,
        EventModels.File? evidenceFile,
        EventModels.RaisedByUserInfo deletedBy,
        DateTime now,
        out MandatoryQualificationDeletedEvent @event)
    {
        if (DeletedOn is not null)
        {
            throw new InvalidOperationException("MandatoryQualification is already deleted.");
        }

        DeletedOn = now;
        UpdatedOn = now;

        @event = new MandatoryQualificationDeletedEvent()
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = now,
            RaisedBy = deletedBy,
            PersonId = PersonId,
            MandatoryQualification = EventModels.MandatoryQualification.FromModel(this),
            DeletionReason = deletionReason,
            DeletionReasonDetail = deletionReasonDetail,
            EvidenceFile = evidenceFile,
        };
    }

    public void Update(
        Action<MandatoryQualification> updateAction,
        string? changeReason,
        string? changeReasonDetail,
        EventModels.File? evidenceFile,
        EventModels.RaisedByUserInfo updatedBy,
        DateTime now,
        out MandatoryQualificationUpdatedEvent? @event)
    {
        var oldMqEventModel = EventModels.MandatoryQualification.FromModel(this);

        updateAction(this);

        var changes = MandatoryQualificationUpdatedEventChanges.None |
            (ProviderId != oldMqEventModel.Provider?.MandatoryQualificationProviderId ? MandatoryQualificationUpdatedEventChanges.Provider : 0) |
            (Specialism != oldMqEventModel.Specialism ? MandatoryQualificationUpdatedEventChanges.Specialism : 0) |
            (Status != oldMqEventModel.Status ? MandatoryQualificationUpdatedEventChanges.Status : 0) |
            (StartDate != oldMqEventModel.StartDate ? MandatoryQualificationUpdatedEventChanges.StartDate : 0) |
            (EndDate != oldMqEventModel.EndDate ? MandatoryQualificationUpdatedEventChanges.EndDate : 0);

        if (changes == MandatoryQualificationUpdatedEventChanges.None)
        {
            @event = null;
            return;
        }

        UpdatedOn = now;

        @event = new MandatoryQualificationUpdatedEvent()
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = now,
            RaisedBy = updatedBy,
            PersonId = PersonId,
            MandatoryQualification = EventModels.MandatoryQualification.FromModel(
                this,
                providerNameHint: ProviderId is Guid providerId ? MandatoryQualificationProvider.GetById(providerId).Name : null),
            OldMandatoryQualification = oldMqEventModel,
            ChangeReason = changeReason,
            ChangeReasonDetail = changeReasonDetail,
            EvidenceFile = evidenceFile,
            Changes = changes
        };
    }
}
