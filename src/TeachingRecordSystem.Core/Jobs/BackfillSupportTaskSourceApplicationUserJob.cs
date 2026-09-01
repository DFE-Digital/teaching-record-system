using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using LegacyEventBase = TeachingRecordSystem.Core.Events.Legacy.EventBase;
using LegacySupportTaskCreatedEvent = TeachingRecordSystem.Core.Events.Legacy.SupportTaskCreatedEvent;
using LegacySupportTaskUpdatedEvent = TeachingRecordSystem.Core.Events.Legacy.SupportTaskUpdatedEvent;

namespace TeachingRecordSystem.Core.Jobs;

/// <summary>
/// Back-fills <see cref="SupportTask.SourceApplicationUserId"/> on tasks that pre-date the column, and on the
/// events that carry a copy of each task.
///
/// Every task type already records the application user it came from somewhere, so the value is recoverable
/// without guessing:
///
/// <list type="bullet">
/// <item>TRN request tasks — including NPQ ones, whose data carries no application user of its own — plus manual
/// checks and Teachers' Pensions duplicates all link to the TRN request that produced them, so the request's
/// application user is the source.</item>
/// <item>One Login identity verification and record matching tasks keep the calling client's application user in
/// their data.</item>
/// <item>Change of name and date of birth requests don't link to a TRN request and carry no application user of
/// their own, but they only ever reach the API from Access your teaching qualifications, so they all take that
/// service's application user.</item>
/// </list>
///
/// A task is only updated when the resolved user still exists and is an application user rather than a staff or
/// system user; anything unresolved is left null and counted in the summary, so a dry run says how much of the
/// table the job can actually account for.
///
/// The source never changes once a task exists, so every event that embeds a copy of the task gets the same
/// value — including the pre-change state on an update, and the events of tasks that already carried the column
/// before it reached the event payloads.
/// </summary>
public class BackfillSupportTaskSourceApplicationUserJob(
    TrsDbContext dbContext,
    ILogger<BackfillSupportTaskSourceApplicationUserJob> logger)
{
    // The only service that raises change requests, and so the source for every one of them.
    private const string ChangeRequestApplicationUserName = "Access your teaching qualifications";

    // Every event that embeds a copy of the task, in each pipeline.
    private static readonly string[] _supportTaskProcessEventNames =
    [
        typeof(SupportTaskCreatedEvent).Name,
        typeof(SupportTaskUpdatedEvent).Name,
        typeof(SupportTaskDeletedEvent).Name
    ];

    private static readonly string[] _legacySupportTaskEventNames =
    [
        typeof(LegacySupportTaskCreatedEvent).Name,
        .. LegacyEventBase.GetEventNamesForBaseType(typeof(LegacySupportTaskUpdatedEvent))
    ];

    // The properties on those events that hold a copy of the task.
    private static readonly string[] _supportTaskPropertyNames =
        [nameof(SupportTaskUpdatedEvent.SupportTask), nameof(SupportTaskUpdatedEvent.OldSupportTask)];

    public async Task ExecuteAsync(bool dryRun, CancellationToken cancellationToken)
    {
        dbContext.Database.SetCommandTimeout(0);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var supportTasks = await dbContext.SupportTasks
            .IgnoreQueryFilters([SupportTask.QueryFilterNames.Deleted])
            .Where(t => t.SourceApplicationUserId == null)
            .ToListAsync(cancellationToken);

        // Old data can name a user that has since been removed, and the process behind a change request could
        // have been a staff user; assigning either would break the foreign key or the navigation property.
        var applicationUserIds = (await dbContext.ApplicationUsers
                .IgnoreQueryFilters()
                .Select(u => u.UserId)
                .ToArrayAsync(cancellationToken))
            .ToHashSet();

        var changeRequestApplicationUserId = await GetChangeRequestApplicationUserIdAsync(cancellationToken);

        // Tasks that already carry the column still have events that pre-date it, so they belong in the map too.
        var sources = (await dbContext.SupportTasks
                .IgnoreQueryFilters([SupportTask.QueryFilterNames.Deleted])
                .Where(t => t.SourceApplicationUserId != null)
                .Select(t => new { t.SupportTaskReference, SourceApplicationUserId = t.SourceApplicationUserId!.Value })
                .ToListAsync(cancellationToken))
            .ToDictionary(t => t.SupportTaskReference, t => t.SourceApplicationUserId);

        var updated = 0;
        var unresolved = new Dictionary<SupportTaskType, int>();

        foreach (var supportTask in supportTasks)
        {
            var sourceApplicationUserId = GetSourceApplicationUserId(supportTask, changeRequestApplicationUserId);

            if (sourceApplicationUserId is not Guid userId || !applicationUserIds.Contains(userId))
            {
                unresolved[supportTask.SupportTaskType] = unresolved.GetValueOrDefault(supportTask.SupportTaskType) + 1;
                continue;
            }

            // The property is init-only, so go through the entry rather than the model.
            dbContext.Entry(supportTask).Property(t => t.SourceApplicationUserId).CurrentValue = userId;
            sources[supportTask.SupportTaskReference] = userId;
            updated++;
        }

        var updatedEvents = await BackfillEventsAsync(sources, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Back-filled the source application user on {UpdatedCount} of {TotalCount} support task(s) and " +
            "{UpdatedEventCount} event(s){DryRunSuffix}.",
            updated,
            supportTasks.Count,
            updatedEvents,
            dryRun ? " (dry run, rolling back)" : "");

        foreach (var (supportTaskType, count) in unresolved.OrderByDescending(u => u.Value))
        {
            logger.LogWarning(
                "Could not resolve the source application user for {Count} support task(s) of type {SupportTaskType}.",
                count,
                supportTaskType);
        }

        if (dryRun)
        {
            await transaction.RollbackAsync(cancellationToken);
        }
        else
        {
            await transaction.CommitAsync(cancellationToken);
        }
    }

    private static Guid? GetSourceApplicationUserId(
        SupportTask supportTask,
        Guid? changeRequestApplicationUserId) => supportTask.SupportTaskType switch
        {
            // All of these are raised off the back of a TRN request, so they inherit its application user.
            // NPQ tasks are historical — the journey's UI was removed in #3434 — but their link is the same.
            SupportTaskType.TrnRequest or
            SupportTaskType.NpqTrnRequest or
            SupportTaskType.TrnRequestManualChecksNeeded or
            SupportTaskType.TeacherPensionsPotentialDuplicate => supportTask.TrnRequestApplicationUserId,

            SupportTaskType.OneLoginUserRecordMatching =>
                supportTask.GetData<OneLoginUserRecordMatchingData>().ClientApplicationUserId,

            SupportTaskType.OneLoginUserIdVerification =>
                supportTask.GetData<OneLoginUserIdVerificationData>().ClientApplicationUserId,

            SupportTaskType.ChangeNameRequest or
            SupportTaskType.ChangeDateOfBirthRequest => changeRequestApplicationUserId,

            var supportTaskType => throw new NotSupportedException(
                $"Cannot derive the source application user for a support task of type '{supportTaskType}'.")
        };

    private async Task<int> BackfillEventsAsync(
        IReadOnlyDictionary<string, Guid> sources,
        CancellationToken cancellationToken)
    {
        if (sources.Count == 0)
        {
            return 0;
        }

        var updated = 0;

        // New pipeline: the payload round-trips through the typed model.
        var processEvents = await dbContext.ProcessEvents
            .Where(e => _supportTaskProcessEventNames.Contains(e.EventName))
            .ToListAsync(cancellationToken);

        foreach (var processEvent in processEvents)
        {
            if (WithSourceApplicationUser(processEvent.Payload, sources) is not { } payload)
            {
                continue;
            }

            dbContext.Entry(processEvent).Property(e => e.Payload).CurrentValue = payload;
            updated++;
        }

        // Legacy events are stored as JSON, so edit the payload in place rather than round-tripping it through
        // the typed model.
        var legacyEvents = await dbContext.Events
            .Where(e => _legacySupportTaskEventNames.Contains(e.EventName))
            .ToListAsync(cancellationToken);

        foreach (var legacyEvent in legacyEvents)
        {
            var payload = JsonNode.Parse(legacyEvent.Payload)!;
            var changed = false;

            foreach (var propertyName in _supportTaskPropertyNames)
            {
                if (payload[propertyName] is JsonObject supportTaskNode &&
                    supportTaskNode["SourceApplicationUserId"] is null &&
                    supportTaskNode["SupportTaskReference"]?.GetValue<string>() is { } reference &&
                    sources.TryGetValue(reference, out var userId))
                {
                    supportTaskNode["SourceApplicationUserId"] = userId;
                    changed = true;
                }
            }

            if (changed)
            {
                dbContext.Entry(legacyEvent).Property(e => e.Payload).CurrentValue = payload.ToJsonString();
                updated++;
            }
        }

        return updated;
    }

    private static IEvent? WithSourceApplicationUser(IEvent @event, IReadOnlyDictionary<string, Guid> sources)
    {
        switch (@event)
        {
            case SupportTaskCreatedEvent created
                when WithSourceApplicationUser(created.SupportTask, sources) is { } supportTask:
                return created with { SupportTask = supportTask };

            case SupportTaskDeletedEvent deleted
                when WithSourceApplicationUser(deleted.SupportTask, sources) is { } supportTask:
                return deleted with { SupportTask = supportTask };

            case SupportTaskUpdatedEvent updated:
                {
                    var supportTask = WithSourceApplicationUser(updated.SupportTask, sources);
                    var oldSupportTask = WithSourceApplicationUser(updated.OldSupportTask, sources);

                    return supportTask is null && oldSupportTask is null
                        ? null
                        : updated with
                        {
                            SupportTask = supportTask ?? updated.SupportTask,
                            OldSupportTask = oldSupportTask ?? updated.OldSupportTask
                        };
                }

            default:
                return null;
        }
    }

    private static EventModels.SupportTask? WithSourceApplicationUser(
        EventModels.SupportTask supportTask,
        IReadOnlyDictionary<string, Guid> sources) =>
        supportTask.SourceApplicationUserId is null &&
        sources.TryGetValue(supportTask.SupportTaskReference, out var userId)
            ? supportTask with { SourceApplicationUserId = userId }
            : null;

    private async Task<Guid?> GetChangeRequestApplicationUserIdAsync(CancellationToken cancellationToken)
    {
        var applicationUsers = await dbContext.ApplicationUsers
            .IgnoreQueryFilters()
            .Where(u => u.Name == ChangeRequestApplicationUserName)
            .Select(u => u.UserId)
            .ToListAsync(cancellationToken);

        if (applicationUsers.Count > 1)
        {
            throw new InvalidOperationException(
                $"Found {applicationUsers.Count} application users named '{ChangeRequestApplicationUserName}'; " +
                "cannot tell which one raised the change requests.");
        }

        if (applicationUsers.Count == 0)
        {
            logger.LogWarning(
                "No application user named '{ApplicationUserName}' exists, so change requests cannot be resolved.",
                ChangeRequestApplicationUserName);

            return null;
        }

        return applicationUsers[0];
    }
}
