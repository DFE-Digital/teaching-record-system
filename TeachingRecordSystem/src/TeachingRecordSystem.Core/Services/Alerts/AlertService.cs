using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Services.Alerts;

public class AlertService(TrsDbContext dbContext, IEventPublisher eventPublisher)
{
    public async Task<Alert> CreateAlertAsync(CreateAlertOptions options, ProcessContext processContext)
    {
        var alert = new Alert
        {
            AlertId = Guid.NewGuid(),
            AlertTypeId = options.AlertTypeId,
            PersonId = options.PersonId,
            Details = options.Details,
            ExternalLink = options.ExternalLink,
            StartDate = options.StartDate,
            EndDate = options.EndDate,
            CreatedOn = processContext.Now,
            UpdatedOn = processContext.Now
        };

        dbContext.Alerts.Add(alert);
        await dbContext.SaveChangesAsync();

        await eventPublisher.PublishEventAsync(
            new AlertCreatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = alert.PersonId,
                Alert = EventModels.Alert.FromModel(alert),
                Reason = options.Reason,
                ReasonDetails = options.ReasonDetails,
                EvidenceFile = options.EvidenceFile
            },
            processContext);

        return alert;
    }

    public async Task DeleteAlertAsync(DeleteAlertOptions options, ProcessContext processContext)
    {
        var alert = await dbContext.Alerts.SingleAsync(a => a.AlertId == options.AlertId);

        alert.DeletedOn = processContext.Now;

        await dbContext.SaveChangesAsync();

        await eventPublisher.PublishEventAsync(
            new AlertDeletedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = alert.PersonId,
                Alert = EventModels.Alert.FromModel(alert),
                ReasonDetails = options.ReasonDetails,
                EvidenceFile = options.EvidenceFile
            },
            processContext);
    }

    public async Task<Alert> UpdateAlertAsync(UpdateAlertOptions options, ProcessContext processContext)
    {
        var alert = await dbContext.Alerts.SingleAsync(a => a.AlertId == options.AlertId);

        var oldAlert = EventModels.Alert.FromModel(alert);
        var changes = AlertUpdatedEventChanges.None;

        options.Details.MatchSome(d =>
        {
            alert.Details = d;
            changes |= AlertUpdatedEventChanges.Details;
        });

        options.ExternalLink.MatchSome(el =>
        {
            alert.ExternalLink = el;
            changes |= AlertUpdatedEventChanges.ExternalLink;
        });

        options.StartDate.MatchSome(sd =>
        {
            alert.StartDate = sd;
            changes |= AlertUpdatedEventChanges.StartDate;
        });

        options.EndDate.MatchSome(ed =>
        {
            alert.EndDate = ed;
            changes |= AlertUpdatedEventChanges.EndDate;
        });

        if (changes != AlertUpdatedEventChanges.None)
        {
            throw new InvalidOperationException("No data has changed.");
        }

        await dbContext.SaveChangesAsync();

        await eventPublisher.PublishEventAsync(
            new AlertUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = alert.PersonId,
                Alert = EventModels.Alert.FromModel(alert),
                OldAlert = oldAlert,
                Reason = options.Reason,
                ReasonDetails = options.ReasonDetails,
                EvidenceFile = options.EvidenceFile,
                Changes = changes
            },
            processContext);

        return alert;
    }
}
