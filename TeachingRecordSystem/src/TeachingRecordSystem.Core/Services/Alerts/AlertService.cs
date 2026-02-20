using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Services.Alerts;

public class AlertService(
    TrsDbContext dbContext,
    IClock clock,
    IEventPublisher eventPublisher)
{
    public async Task<Alert> CreateAlertAsync(CreateAlertOptions options, ProcessContext processContext)
    {
        await using var eventScope = eventPublisher.GetOrCreateEventScope(processContext);

        var now = clock.UtcNow;
        var alertId = Guid.NewGuid();

        var alert = new Alert
        {
            AlertId = alertId,
            PersonId = options.PersonId,
            AlertTypeId = options.AlertTypeId,
            Details = options.Details,
            ExternalLink = options.ExternalLink,
            StartDate = options.StartDate,
            EndDate = options.EndDate,
            CreatedOn = now,
            UpdatedOn = now
        };

        dbContext.Add(alert);
        await dbContext.SaveChangesAsync();

        await eventScope.PublishEventAsync(
            new AlertCreatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = options.PersonId,
                Alert = EventModels.Alert.FromModel(alert)
            });

        return alert;
    }

    public async Task DeleteAlertAsync(DeleteAlertOptions options, ProcessContext processContext)
    {
        var alert = await dbContext.Alerts.FindOrThrowAsync(options.AlertId);

        if (alert.DeletedOn is not null)
        {
            throw new InvalidOperationException("Alert is already deleted.");
        }

        await using var eventScope = eventPublisher.GetOrCreateEventScope(processContext);

        var now = clock.UtcNow;
        alert.DeletedOn = now;
        alert.UpdatedOn = now;

        await dbContext.SaveChangesAsync();

        await eventScope.PublishEventAsync(
            new AlertDeletedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = alert.PersonId,
                Alert = EventModels.Alert.FromModel(alert)
            });
    }

    public async Task<AlertUpdatedEventChanges> UpdateAlertAsync(UpdateAlertOptions options, ProcessContext processContext)
    {
        var alert = await dbContext.Alerts.FindOrThrowAsync(options.AlertId);

        if (alert.DeletedOn is not null)
        {
            throw new InvalidOperationException("Cannot update a deleted alert.");
        }

        await using var eventScope = eventPublisher.GetOrCreateEventScope(processContext);

        var oldAlert = EventModels.Alert.FromModel(alert);

        options.Details.MatchSome(details => alert.Details = details);
        options.ExternalLink.MatchSome(externalLink => alert.ExternalLink = externalLink);
        options.StartDate.MatchSome(startDate => alert.StartDate = startDate);
        options.EndDate.MatchSome(endDate => alert.EndDate = endDate);

        var changes = AlertUpdatedEventChanges.None |
            (alert.Details != oldAlert.Details ? AlertUpdatedEventChanges.Details : 0) |
            (alert.ExternalLink != oldAlert.ExternalLink ? AlertUpdatedEventChanges.ExternalLink : 0) |
            (alert.StartDate != oldAlert.StartDate ? AlertUpdatedEventChanges.StartDate : 0) |
            (alert.EndDate != oldAlert.EndDate ? AlertUpdatedEventChanges.EndDate : 0);

        if (changes == AlertUpdatedEventChanges.None)
        {
            return changes;
        }

        var now = clock.UtcNow;
        alert.UpdatedOn = now;

        await dbContext.SaveChangesAsync();

        await eventScope.PublishEventAsync(
            new AlertUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = alert.PersonId,
                Alert = EventModels.Alert.FromModel(alert),
                OldAlert = oldAlert,
                Changes = changes
            });

        return changes;
    }
}
