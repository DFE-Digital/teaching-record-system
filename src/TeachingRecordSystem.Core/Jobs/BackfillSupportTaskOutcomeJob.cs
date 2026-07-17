using System.Text.Json.Nodes;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using LegacyEventBase = TeachingRecordSystem.Core.Events.Legacy.EventBase;
using LegacySupportTaskUpdatedEvent = TeachingRecordSystem.Core.Events.Legacy.SupportTaskUpdatedEvent;
using SupportTaskUpdatedEvent = TeachingRecordSystem.Core.Events.SupportTaskUpdatedEvent;

namespace TeachingRecordSystem.Core.Jobs;

/// <summary>
/// Back-fills <see cref="SupportTask.Outcome"/> on closed tasks that pre-date the column, and on the event that
/// closed each of them.
///
/// The outcome was previously derived on demand from the task's data as a free-text label, so it's recoverable
/// the same way: every resolve journey records enough on the task to tell its outcomes apart. Where a journey
/// resolved against an existing record it left a <c>SelectedPersonAttributes</c> snapshot behind, and where it
/// created a new one it didn't, which is what separates the two TRN request and Teachers' Pensions outcomes.
///
/// Open and in-progress tasks have no outcome and are skipped.
///
/// The NPQ journey's UI was removed in #3434, so those tasks are historical only — they're included because
/// their data still needs an outcome, not because anything still writes it.
/// </summary>
public class BackfillSupportTaskOutcomeJob(TrsDbContext dbContext)
{
    // Legacy events that carry the support task's before/after state, i.e. can represent a close.
    private static readonly string[] _closingLegacyEventNames =
        [.. LegacyEventBase.GetEventNamesForBaseType(typeof(LegacySupportTaskUpdatedEvent))];

    public async Task ExecuteAsync(bool dryRun, CancellationToken cancellationToken)
    {
        dbContext.Database.SetCommandTimeout(0);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var supportTasks = await dbContext.SupportTasks
            .IgnoreQueryFilters()
            .Where(t => t.Status == SupportTaskStatus.Closed)
            .Where(t => t.Outcome == null)
            .ToListAsync(cancellationToken);

        // The outcome assigned to each task we changed, keyed by reference, so the event that closed it can be
        // brought in line with it.
        var outcomes = new Dictionary<string, SupportTaskOutcome>();

        foreach (var supportTask in supportTasks)
        {
            var outcome = GetOutcome(supportTask);
            supportTask.Outcome = outcome;
            outcomes.Add(supportTask.SupportTaskReference, outcome);
        }

        await BackfillClosingEventsAsync(outcomes, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        if (dryRun)
        {
            await transaction.RollbackAsync(cancellationToken);
        }
        else
        {
            await transaction.CommitAsync(cancellationToken);
        }
    }

    private static SupportTaskOutcome GetOutcome(SupportTask supportTask) => supportTask.SupportTaskType switch
    {
        SupportTaskType.ChangeNameRequest => supportTask.GetData<ChangeNameRequestData>().ChangeRequestOutcome switch
        {
            SupportRequestOutcome.Approved => SupportTaskOutcome.ChangeNameRequest_Approved,
            SupportRequestOutcome.Rejected => SupportTaskOutcome.ChangeNameRequest_Rejected,
            SupportRequestOutcome.Cancelled => SupportTaskOutcome.ChangeNameRequest_Cancelled,
            var outcome => throw CannotDeriveOutcome(supportTask, $"{nameof(ChangeNameRequestData.ChangeRequestOutcome)} is '{outcome?.ToString() ?? "null"}'")
        },

        SupportTaskType.ChangeDateOfBirthRequest => supportTask.GetData<ChangeDateOfBirthRequestData>().ChangeRequestOutcome switch
        {
            SupportRequestOutcome.Approved => SupportTaskOutcome.ChangeDateOfBirthRequest_Approved,
            SupportRequestOutcome.Rejected => SupportTaskOutcome.ChangeDateOfBirthRequest_Rejected,
            SupportRequestOutcome.Cancelled => SupportTaskOutcome.ChangeDateOfBirthRequest_Cancelled,
            var outcome => throw CannotDeriveOutcome(supportTask, $"{nameof(ChangeDateOfBirthRequestData.ChangeRequestOutcome)} is '{outcome?.ToString() ?? "null"}'")
        },

        SupportTaskType.OneLoginUserIdVerification => supportTask.GetData<OneLoginUserIdVerificationData>().Outcome switch
        {
            OneLoginUserIdVerificationOutcome.NotVerified => SupportTaskOutcome.OneLoginUserIdVerification_NotVerified,
            OneLoginUserIdVerificationOutcome.VerifiedOnlyWithMatches => SupportTaskOutcome.OneLoginUserIdVerification_VerifiedOnlyWithMatches,
            OneLoginUserIdVerificationOutcome.VerifiedOnlyWithoutMatches => SupportTaskOutcome.OneLoginUserIdVerification_VerifiedOnlyWithoutMatches,
            OneLoginUserIdVerificationOutcome.VerifiedAndConnected => SupportTaskOutcome.OneLoginUserIdVerification_VerifiedAndConnected,
            var outcome => throw CannotDeriveOutcome(supportTask, $"{nameof(OneLoginUserIdVerificationData.Outcome)} is '{outcome?.ToString() ?? "null"}'")
        },

        SupportTaskType.OneLoginUserRecordMatching => supportTask.GetData<OneLoginUserRecordMatchingData>().Outcome switch
        {
            OneLoginUserRecordMatchingOutcome.NotConnecting => SupportTaskOutcome.OneLoginUserRecordMatching_NotConnecting,
            OneLoginUserRecordMatchingOutcome.NoMatches => SupportTaskOutcome.OneLoginUserRecordMatching_NoMatches,
            OneLoginUserRecordMatchingOutcome.Connected => SupportTaskOutcome.OneLoginUserRecordMatching_Connected,
            var outcome => throw CannotDeriveOutcome(supportTask, $"{nameof(OneLoginUserRecordMatchingData.Outcome)} is '{outcome?.ToString() ?? "null"}'")
        },

        // Only a resolve against an existing record takes a snapshot of that record.
        SupportTaskType.TrnRequest => supportTask.GetData<TrnRequestData>().SelectedPersonAttributes is not null
            ? SupportTaskOutcome.TrnRequest_ResolvedWithExistingPerson
            : SupportTaskOutcome.TrnRequest_ResolvedWithNewPerson,

        SupportTaskType.NpqTrnRequest => GetNpqTrnRequestOutcome(supportTask),

        SupportTaskType.TrnRequestManualChecksNeeded => SupportTaskOutcome.TrnRequestManualChecksNeeded_Completed,

        // Only a resolve that merged the records takes a snapshot of the record kept.
        SupportTaskType.TeacherPensionsPotentialDuplicate =>
            supportTask.GetData<TeacherPensionsPotentialDuplicateData>().SelectedPersonAttributes is not null
                ? SupportTaskOutcome.TeacherPensionsPotentialDuplicate_ResolvedWithMerge
                : SupportTaskOutcome.TeacherPensionsPotentialDuplicate_ResolvedWithoutMerge,

        var supportTaskType => throw new NotSupportedException(
            $"Cannot derive the outcome for a support task of type '{supportTaskType}'.")
    };

    private static SupportTaskOutcome GetNpqTrnRequestOutcome(SupportTask supportTask)
    {
        var data = supportTask.GetData<NpqTrnRequestData>();

        return data.SupportRequestOutcome switch
        {
            // The NPQ journey never offered a name choice, so an approved request that identified an existing
            // record still snapshots it; only a newly created record has no snapshot.
            SupportRequestOutcome.Approved => data.SelectedPersonAttributes is not null
                ? SupportTaskOutcome.NpqTrnRequest_ResolvedWithExistingPerson
                : SupportTaskOutcome.NpqTrnRequest_ResolvedWithNewPerson,
            SupportRequestOutcome.Rejected => SupportTaskOutcome.NpqTrnRequest_Rejected,
            var outcome => throw CannotDeriveOutcome(supportTask, $"{nameof(NpqTrnRequestData.SupportRequestOutcome)} is '{outcome?.ToString() ?? "null"}'")
        };
    }

    private static InvalidOperationException CannotDeriveOutcome(SupportTask supportTask, string reason) =>
        new($"Cannot derive the outcome for support task '{supportTask.SupportTaskReference}' of type " +
            $"'{supportTask.SupportTaskType}': {reason}.");

    private async Task BackfillClosingEventsAsync(
        IReadOnlyDictionary<string, SupportTaskOutcome> outcomes,
        CancellationToken cancellationToken)
    {
        if (outcomes.Count == 0)
        {
            return;
        }

        // New pipeline: the close is a SupportTaskUpdatedEvent in process_events.
        var processEvents = await dbContext.ProcessEvents
            .Where(e => e.EventName == nameof(SupportTaskUpdatedEvent))
            .ToListAsync(cancellationToken);

        foreach (var processEvent in processEvents)
        {
            if (processEvent.Payload is SupportTaskUpdatedEvent payload &&
                outcomes.TryGetValue(payload.SupportTask.SupportTaskReference, out var outcome) &&
                ClosesTask(payload.OldSupportTask.Status, payload.SupportTask.Status) &&
                payload.SupportTask.Outcome is null)
            {
                dbContext.Entry(processEvent).Property(e => e.Payload).CurrentValue = payload with
                {
                    Changes = payload.Changes | SupportTaskUpdatedEventChanges.Outcome,
                    SupportTask = payload.SupportTask with { Outcome = outcome }
                };
            }
        }

        // Legacy events are stored as JSON, so edit the payload in place rather than round-tripping it
        // through the typed model. Their Changes enums have no outcome flag, so only the task is updated.
        var legacyEvents = await dbContext.Events
            .Where(e => _closingLegacyEventNames.Contains(e.EventName))
            .ToListAsync(cancellationToken);

        foreach (var legacyEvent in legacyEvents)
        {
            if (legacyEvent.ToEventBase() is not LegacySupportTaskUpdatedEvent { SupportTask: var newTask, OldSupportTask: var oldTask } ||
                !ClosesTask(oldTask.Status, newTask.Status) ||
                !outcomes.TryGetValue(newTask.SupportTaskReference, out var outcome))
            {
                continue;
            }

            var payload = JsonNode.Parse(legacyEvent.Payload)!;

            if (payload["SupportTask"] is not JsonObject supportTaskNode)
            {
                continue;
            }

            supportTaskNode["Outcome"] = (int)outcome;

            // The label these payloads carried is no longer part of the model.
            supportTaskNode.Remove("OutcomeLabel");
            (payload["OldSupportTask"] as JsonObject)?.Remove("OutcomeLabel");

            dbContext.Entry(legacyEvent).Property(e => e.Payload).CurrentValue = payload.ToJsonString();
        }
    }

    private static bool ClosesTask(SupportTaskStatus oldStatus, SupportTaskStatus newStatus) =>
        oldStatus is not SupportTaskStatus.Closed && newStatus is SupportTaskStatus.Closed;
}
