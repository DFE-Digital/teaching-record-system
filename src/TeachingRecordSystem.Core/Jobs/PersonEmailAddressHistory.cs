using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Jobs;

/// <summary>
/// Reconstructs the email address that was on a person's record at a point in time from the events
/// that have changed it since, so back-fills don't attribute today's address to an old change.
/// </summary>
public class PersonEmailAddressHistory
{
    // Legacy events carrying a snapshot of the person's attributes as they were before the change.
    private static readonly string[] _legacyEventNames = typeof(LegacyEvents.IEventWithPersonAttributes).Assembly
        .GetTypes()
        .Where(t => t is { IsClass: true, IsAbstract: false } && t.IsAssignableTo(typeof(LegacyEvents.IEventWithPersonAttributes)))
        .Select(t => t.Name)
        .ToArray();

    // Not every change made through a process writes a legacy event, so we have to read both stores.
    private static readonly string[] _processEventNames =
    [
        nameof(PersonDetailsUpdatedEvent),
        nameof(PersonUpdatedInDqtEvent)
    ];

    private readonly Dictionary<Guid, (DateTime RecordedOn, string? EmailAddress)[]> _emailAddressesBeforeChanges;
    private readonly Dictionary<Guid, string?> _currentEmailAddresses;

    private PersonEmailAddressHistory(
        Dictionary<Guid, (DateTime RecordedOn, string? EmailAddress)[]> emailAddressesBeforeChanges,
        Dictionary<Guid, string?> currentEmailAddresses)
    {
        _emailAddressesBeforeChanges = emailAddressesBeforeChanges;
        _currentEmailAddresses = currentEmailAddresses;
    }

    public static async Task<PersonEmailAddressHistory> CreateAsync(
        TrsDbContext dbContext,
        Guid[] personIds,
        CancellationToken cancellationToken)
    {
        var wanted = personIds.ToHashSet();
        var emailAddressesBeforeChanges = new Dictionary<Guid, List<(DateTime RecordedOn, string? EmailAddress)>>();

        void AddChange(Guid personId, DateTime recordedOn, string? emailAddressBefore)
        {
            if (!wanted.Contains(personId))
            {
                return;
            }

            if (!emailAddressesBeforeChanges.TryGetValue(personId, out var changes))
            {
                changes = [];
                emailAddressesBeforeChanges.Add(personId, changes);
            }

            changes.Add((recordedOn, emailAddressBefore));
        }

        var legacyEvents = await dbContext.Events
            .Where(e => _legacyEventNames.Contains(e.EventName) && e.PersonIds.Any(id => personIds.Contains(id)))
            .Select(e => new { e.EventName, e.Payload, e.Created })
            .ToListAsync(cancellationToken);

        foreach (var legacyEvent in legacyEvents)
        {
            // An event without a previous snapshot — a creation — tells us nothing about what came before it.
            if (LegacyEvents.EventBase.Deserialize(legacyEvent.Payload, legacyEvent.EventName)
                is LegacyEvents.IEventWithPersonAttributes { OldPersonAttributes: { } oldPersonAttributes } eventWithPersonAttributes)
            {
                AddChange(eventWithPersonAttributes.PersonId, legacyEvent.Created, oldPersonAttributes.EmailAddress);
            }
        }

        var processEvents = await dbContext.ProcessEvents
            .Where(pe => _processEventNames.Contains(pe.EventName) && pe.PersonIds.Any(id => personIds.Contains(id)))
            .Select(pe => new { pe.Payload, pe.CreatedOn })
            .ToListAsync(cancellationToken);

        foreach (var processEvent in processEvents)
        {
            switch (processEvent.Payload)
            {
                case PersonDetailsUpdatedEvent updated:
                    AddChange(updated.PersonId, processEvent.CreatedOn, updated.OldPersonDetails.EmailAddress);
                    break;
                case PersonUpdatedInDqtEvent updated:
                    AddChange(updated.PersonId, processEvent.CreatedOn, updated.OldDetails.EmailAddress);
                    break;
            }
        }

        // The person may have been deactivated since.
        var currentEmailAddresses = await dbContext.Persons
            .IgnoreQueryFilters([Person.QueryFilterNames.Deactivated])
            .Where(p => personIds.Contains(p.PersonId))
            .ToDictionaryAsync(p => p.PersonId, p => p.EmailAddress, cancellationToken);

        return new PersonEmailAddressHistory(
            emailAddressesBeforeChanges.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.OrderBy(c => c.RecordedOn).ToArray()),
            currentEmailAddresses);
    }

    /// <summary>
    /// The address in force at <paramref name="at"/> is the one the first change made after it recorded
    /// as the old value; where nothing has changed it since, it's the address on the record today.
    /// </summary>
    public string? GetEmailAddressAt(Guid personId, DateTime at)
    {
        if (_emailAddressesBeforeChanges.TryGetValue(personId, out var changes))
        {
            foreach (var change in changes)
            {
                if (change.RecordedOn > at)
                {
                    return change.EmailAddress;
                }
            }
        }

        return _currentEmailAddresses.GetValueOrDefault(personId);
    }
}
