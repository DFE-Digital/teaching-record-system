using EntityFrameworkCore.Projectables;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class Alert
{
    public const string AlertTypeIdIndexName = "ix_alerts_alert_type_id";
    public const string AlertTypeForeignKeyName = "fk_alerts_alert_type";
    public const string PersonIdIndexName = "ix_alerts_person_id";
    public const string PersonForeignKeyName = "fk_alerts_person";

    public required Guid AlertId { get; init; }
    public AlertType AlertType { get; } = null!;
    public required Guid AlertTypeId { get; init; }
    public required Guid PersonId { get; init; }
    public Person Person { get; } = null!;
    public required string? Details { get; set; }
    public required string? ExternalLink { get; set; }
    public required DateOnly? StartDate { get; set; }
    public required DateOnly? EndDate { get; set; }
    public required DateTime CreatedOn { get; init; }
    public required DateTime UpdatedOn { get; set; }
    public DateTime? DeletedOn { get; set; }
    [Projectable] public bool IsOpen => EndDate == null;

    public static Alert Create(
        Guid alertTypeId,
        Guid personId,
        string? details,
        string? externalLink,
        DateOnly startDate,
        DateOnly? endDate,
        string? addReason,
        string? addReasonDetail,
        EventModels.File? evidenceFile,
        EventModels.RaisedByUserInfo createdBy,
        DateTime now,
        out AlertCreatedEvent @event)
    {
        var alertId = Guid.NewGuid();

        var alert = new Alert()
        {
            AlertId = alertId,
            PersonId = personId,
            AlertTypeId = alertTypeId,
            Details = details,
            ExternalLink = externalLink,
            StartDate = startDate,
            EndDate = endDate,
            CreatedOn = now,
            UpdatedOn = now
        };

        @event = new AlertCreatedEvent()
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = now,
            RaisedBy = createdBy,
            Alert = EventModels.Alert.FromModel(alert),
            PersonId = personId,
            AddReason = addReason,
            AddReasonDetail = addReasonDetail,
            EvidenceFile = evidenceFile
        };

        return alert;
    }

    public void Delete(
        string? deletionReasonDetail,
        EventModels.File? evidenceFile,
        EventModels.RaisedByUserInfo deletedBy,
        DateTime now,
        out AlertDeletedEvent @event)
    {
        if (DeletedOn is not null)
        {
            throw new InvalidOperationException("Alert is already deleted.");
        }

        DeletedOn = now;
        UpdatedOn = now;

        @event = new AlertDeletedEvent()
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = now,
            RaisedBy = deletedBy,
            PersonId = PersonId,
            Alert = EventModels.Alert.FromModel(this),
            DeletionReasonDetail = deletionReasonDetail,
            EvidenceFile = evidenceFile,
        };
    }

    public void Update(
        Action<Alert> updateAction,
        string? changeReason,
        string? changeReasonDetail,
        EventModels.File? evidenceFile,
        EventModels.RaisedByUserInfo changedBy,
        DateTime now,
        out AlertUpdatedEvent? @event)
    {
        var oldAlertEventModel = EventModels.Alert.FromModel(this);

        updateAction(this);

        var changes = AlertUpdatedEventChanges.None |
            (Details != oldAlertEventModel.Details ? AlertUpdatedEventChanges.Details : 0) |
            (ExternalLink != oldAlertEventModel.ExternalLink ? AlertUpdatedEventChanges.ExternalLink : 0) |
            (StartDate != oldAlertEventModel.StartDate ? AlertUpdatedEventChanges.StartDate : 0) |
            (EndDate != oldAlertEventModel.EndDate ? AlertUpdatedEventChanges.EndDate : 0);

        if (changes == AlertUpdatedEventChanges.None)
        {
            @event = null;
            return;
        }

        UpdatedOn = now;

        @event = new AlertUpdatedEvent()
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = now,
            RaisedBy = changedBy,
            PersonId = PersonId,
            Alert = EventModels.Alert.FromModel(this),
            OldAlert = oldAlertEventModel,
            ChangeReason = changeReason,
            ChangeReasonDetail = changeReasonDetail,
            EvidenceFile = evidenceFile,
            Changes = changes
        };
    }
}
