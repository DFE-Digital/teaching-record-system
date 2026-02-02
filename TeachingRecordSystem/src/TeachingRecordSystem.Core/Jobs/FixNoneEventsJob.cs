using System.Text.Json;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Core.Jobs;

public class FixNoneEventsJob
{
    private readonly TrsDbContext _dbContext;

    public FixNoneEventsJob(TrsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {

            var rows = await _dbContext.Events
                .FromSqlRaw(@"
                SELECT event_id, payload
                FROM events
                WHERE event_name = 'PersonDetailsUpdatedEvent'
                  AND COALESCE((payload ->> 'Changes')::int, 0) = 16
                ")
                .AsNoTracking()
                .Select(e => new { e.EventId, Payload = e.Payload.ToString() })
                .ToListAsync(cancellationToken);

            foreach (var row in rows)
            {
                var json = row.Payload.Replace("PersonAttributes", "PersonDetails")
                    .Replace("OldPersonAttributes", "OldPersonDetails");
                var evt = JsonSerializer.Deserialize<PersonDetailsUpdatedEvent>(
                    json,
                    jsonOptions);

                if (evt is null)
                {
                    continue;
                }

                var newChanges = CalculateChanges(
                    evt.PersonDetails,
                    evt.OldPersonDetails);

                if (newChanges == Events.Legacy.PersonDetailsUpdatedEventChanges.None)
                {
                    await _dbContext.Database.ExecuteSqlInterpolatedAsync($@"
                    DELETE FROM events
                    WHERE event_id = {row.EventId}
                    AND event_name = 'PersonDetailsUpdatedEvent'
                ", cancellationToken);
                    continue;
                }

                await _dbContext.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE events
                SET payload = jsonb_set(
                    payload,
                    '{{Changes}}',
                    to_jsonb({(int)newChanges}),
                    true
                )
                WHERE event_id = {row.EventId}
                  AND event_name = 'PersonDetailsUpdatedEvent'
                  AND COALESCE((payload ->> 'Changes')::int, 0) = 16
            ", cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {

            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public static Events.Legacy.PersonDetailsUpdatedEventChanges CalculateChanges(
        EventModels.PersonDetails current,
        EventModels.PersonDetails old)
    {
        var changes = Events.Legacy.PersonDetailsUpdatedEventChanges.None;

        if (current.FirstName != old.FirstName)
        {
            changes |= Events.Legacy.PersonDetailsUpdatedEventChanges.FirstName;
        }

        if (current.MiddleName != old.MiddleName)
        {
            changes |= Events.Legacy.PersonDetailsUpdatedEventChanges.MiddleName;
        }

        if (current.LastName != old.LastName)
        {
            changes |= Events.Legacy.PersonDetailsUpdatedEventChanges.LastName;
        }

        if (current.DateOfBirth != old.DateOfBirth)
        {
            changes |= Events.Legacy.PersonDetailsUpdatedEventChanges.DateOfBirth;
        }

        if (!string.Equals(
                current.EmailAddress,
                old.EmailAddress,
                StringComparison.Ordinal))
        {
            changes |= Events.Legacy.PersonDetailsUpdatedEventChanges.EmailAddress;
        }

        if (!string.Equals(
                current.NationalInsuranceNumber,
                old.NationalInsuranceNumber,
                StringComparison.Ordinal))
        {
            changes |= Events.Legacy.PersonDetailsUpdatedEventChanges.NationalInsuranceNumber;
        }

        if (current.Gender != old.Gender)
        {
            changes |= Events.Legacy.PersonDetailsUpdatedEventChanges.Gender;
        }

        return changes;
    }
}
