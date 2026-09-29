using Microsoft.Extensions.Logging;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Jobs;

/// <summary>
/// Re-types the person creations from the since-removed <c>AllocateTrnsToOverseasNpqApplicantsJob</c>, which
/// were back-filled onto processes typed <see cref="ProcessType.TeacherPensionsRecordImporting"/>. They are
/// not Teachers' Pensions imports, and until this runs the people involved show a "Record imported from
/// Teachers' Pensions" entry on their change history for a record that arrived by a different route.
/// </summary>
/// <remarks>
/// The signature is one that cannot occur naturally. <see cref="CapitaImportJob"/> is the only thing that
/// creates this process type, and it runs as the configured Capita Teachers' Pensions user, never as
/// <see cref="SystemUser"/>; a person it creates is stamped <c>CreatedByTps</c>; and its processes hold a
/// <c>SupportTaskCreatedEvent</c> alongside the creation whenever the import flagged a duplicate. A process
/// of this type raised by the system user, holding a single <see cref="PersonCreatedEvent"/> and no support
/// task, for a person not marked as created by TPS, is from that bulk run.
///
/// This job and <see cref="BackfillOverseasNpqTrnEmailSentEventsJob"/> can run in either order: the signature
/// tolerates the <see cref="EmailSentEvent"/> that one attaches, and that job finds its target by the process
/// holding the person's creation rather than by process type.
///
/// They become <see cref="ProcessType.PersonCreating"/> rather than <see cref="ProcessType.TrnAllocating"/>.
/// The allocation put the TRN on the record at the moment the record was created, so there is no separate
/// allocation to describe, and these processes hold no <c>TrnAllocatedEvent</c> for the TRN allocation entry
/// to render from. "Record created", with the person's details, is what happened.
///
/// Anything that doesn't match the full signature is left alone and reported rather than re-typed on a
/// partial match.
/// </remarks>
public class RepairOverseasNpqPersonCreationProcessesJob(
    TrsDbContext dbContext,
    ILogger<RepairOverseasNpqPersonCreationProcessesJob> logger)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        dbContext.Database.SetCommandTimeout(0);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var candidates = await dbContext.Processes
            .Where(p => p.ProcessType == ProcessType.TeacherPensionsRecordImporting)
            .Where(p => p.UserId == SystemUser.SystemUserId)
            .Include(p => p.Events)
            .ToListAsync(cancellationToken);

        if (candidates.Count == 0)
        {
            return;
        }

        var candidatePersonIds = candidates.SelectMany(p => p.PersonIds).Distinct().ToArray();

        // The person may have been deactivated since the TRN was allocated.
        var createdByTps = await dbContext.Persons
            .IgnoreQueryFilters([QueryFilterNames.Person.Deactivated])
            .Where(p => candidatePersonIds.Contains(p.PersonId))
            .ToDictionaryAsync(p => p.PersonId, p => p.CreatedByTps, cancellationToken);

        var repaired = 0;
        var skipped = 0;

        foreach (var process in candidates)
        {
            var payloads = process.Events!.Select(e => e.Payload).ToArray();
            var personCreatedEvents = payloads.OfType<PersonCreatedEvent>().ToArray();

            // An EmailSentEvent alongside the creation is expected — BackfillOverseasNpqTrnEmailSentEventsJob
            // adds one, and either job may run first. A SupportTaskCreatedEvent is not: a real import writes
            // one whenever it flagged a duplicate, and the bulk run raised no tasks.
            if (personCreatedEvents.Length != 1 ||
                payloads.Any(p => p is not (PersonCreatedEvent or EmailSentEvent)) ||
                !createdByTps.TryGetValue(personCreatedEvents[0].PersonId, out var isCreatedByTps) ||
                isCreatedByTps)
            {
                logger.LogWarning(
                    "Process {ProcessId} is raised by the system user but doesn't match the overseas NPQ " +
                    "signature; leaving it alone.",
                    process.ProcessId);

                skipped++;
                continue;
            }

            dbContext.Entry(process).Property(p => p.ProcessType).CurrentValue = ProcessType.PersonCreating;

            repaired++;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        logger.LogInformation(
            "Re-typed {Repaired} overseas NPQ person creation processes; {Skipped} left alone.",
            repaired,
            skipped);
    }
}
