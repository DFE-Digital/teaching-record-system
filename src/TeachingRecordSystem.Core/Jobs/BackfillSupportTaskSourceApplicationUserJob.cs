using Microsoft.Extensions.Logging;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using LegacySupportTaskCreatedEvent = TeachingRecordSystem.Core.Events.Legacy.SupportTaskCreatedEvent;

namespace TeachingRecordSystem.Core.Jobs;

/// <summary>
/// Back-fills <see cref="SupportTask.SourceApplicationUserId"/> on tasks that pre-date the column.
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
/// <item>Change of name and date of birth requests come in through the API and don't link to a TRN request, so
/// the source is the user recorded against the process that created them, falling back to the legacy created
/// event for tasks that pre-date the process pipeline.</item>
/// </list>
///
/// A task is only updated when the resolved user still exists and is an application user rather than a staff or
/// system user; anything unresolved is left null and counted in the summary, so a dry run says how much of the
/// table the job can actually account for.
/// </summary>
public class BackfillSupportTaskSourceApplicationUserJob(
    TrsDbContext dbContext,
    ILogger<BackfillSupportTaskSourceApplicationUserJob> logger)
{
    private static readonly SupportTaskType[] _changeRequestTypes =
        [SupportTaskType.ChangeNameRequest, SupportTaskType.ChangeDateOfBirthRequest];

    private static readonly ProcessType[] _changeRequestCreatingProcessTypes =
        [ProcessType.ChangeOfNameRequestCreating, ProcessType.ChangeOfDateOfBirthRequestCreating];

    // Matches the EventName stored in the events table, which is the type's own name rather than the alias.
    private static readonly string _legacySupportTaskCreatedEventName = typeof(LegacySupportTaskCreatedEvent).Name;

    public async Task ExecuteAsync(bool dryRun, CancellationToken cancellationToken)
    {
        dbContext.Database.SetCommandTimeout(0);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var supportTasks = await dbContext.SupportTasks
            .IgnoreQueryFilters()
            .Where(t => t.SourceApplicationUserId == null)
            .ToListAsync(cancellationToken);

        // Old data can name a user that has since been removed, and the process behind a change request could
        // have been a staff user; assigning either would break the foreign key or the navigation property.
        var applicationUserIds = (await dbContext.ApplicationUsers
                .IgnoreQueryFilters()
                .Select(u => u.UserId)
                .ToArrayAsync(cancellationToken))
            .ToHashSet();

        var changeRequestCreators = await GetChangeRequestCreatorsAsync(supportTasks, cancellationToken);

        var updated = 0;
        var unresolved = new Dictionary<SupportTaskType, int>();

        foreach (var supportTask in supportTasks)
        {
            var sourceApplicationUserId = GetSourceApplicationUserId(supportTask, changeRequestCreators);

            if (sourceApplicationUserId is not Guid userId || !applicationUserIds.Contains(userId))
            {
                unresolved[supportTask.SupportTaskType] = unresolved.GetValueOrDefault(supportTask.SupportTaskType) + 1;
                continue;
            }

            // The property is init-only, so go through the entry rather than the model.
            dbContext.Entry(supportTask).Property(t => t.SourceApplicationUserId).CurrentValue = userId;
            updated++;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Back-filled the source application user on {UpdatedCount} of {TotalCount} support task(s){DryRunSuffix}.",
            updated,
            supportTasks.Count,
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
        IReadOnlyDictionary<string, Guid> changeRequestCreators) => supportTask.SupportTaskType switch
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
            SupportTaskType.ChangeDateOfBirthRequest =>
                changeRequestCreators.TryGetValue(supportTask.SupportTaskReference, out var userId) ? userId : null,

            var supportTaskType => throw new NotSupportedException(
                $"Cannot derive the source application user for a support task of type '{supportTaskType}'.")
        };

    private async Task<IReadOnlyDictionary<string, Guid>> GetChangeRequestCreatorsAsync(
        IReadOnlyCollection<SupportTask> supportTasks,
        CancellationToken cancellationToken)
    {
        var creators = new Dictionary<string, Guid>();

        var references = supportTasks
            .Where(t => _changeRequestTypes.Contains(t.SupportTaskType))
            .Select(t => t.SupportTaskReference)
            .ToHashSet();

        if (references.Count == 0)
        {
            return creators;
        }

        // The API stamps the calling application user on the process that creates the request. There's one
        // process per change request ever made, so match them up in memory rather than pushing the reference
        // list into the array containment query.
        var processes = await dbContext.Processes
            .Where(p => _changeRequestCreatingProcessTypes.Contains(p.ProcessType))
            .Where(p => p.UserId != null)
            .Select(p => new { UserId = p.UserId!.Value, p.SupportTaskReferences })
            .ToListAsync(cancellationToken);

        foreach (var process in processes)
        {
            foreach (var reference in process.SupportTaskReferences.Where(references.Contains))
            {
                creators.TryAdd(reference, process.UserId);
            }
        }

        if (references.All(creators.ContainsKey))
        {
            return creators;
        }

        // Requests made before the process pipeline only left the legacy created event behind, which records
        // the same user. The events table has no support task column, so the payloads have to be read.
        var legacyEvents = await dbContext.Events
            .Where(e => e.EventName == _legacySupportTaskCreatedEventName)
            .ToListAsync(cancellationToken);

        foreach (var legacyEvent in legacyEvents)
        {
            if (legacyEvent.ToEventBase() is LegacySupportTaskCreatedEvent { SupportTask.SupportTaskReference: var reference, RaisedBy.UserId: Guid userId } &&
                references.Contains(reference))
            {
                creators.TryAdd(reference, userId);
            }
        }

        return creators;
    }
}
