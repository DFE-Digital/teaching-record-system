using System.Text.Json;
using System.Text.Json.Nodes;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using LegacyEventBase = TeachingRecordSystem.Core.Events.Legacy.EventBase;
using LegacySupportTaskUpdatedEvent = TeachingRecordSystem.Core.Events.Legacy.SupportTaskUpdatedEvent;
using SupportTaskUpdatedEvent = TeachingRecordSystem.Core.Events.SupportTaskUpdatedEvent;

namespace TeachingRecordSystem.Core.Jobs;

/// <summary>
/// Corrects <c>ResolvedAttributes</c> on closed TRN request, NPQ TRN request and Teachers' Pensions potential
/// duplicate tasks that were resolved by merging into an existing record.
///
/// The resolve journeys recorded the TRN request's value for any attribute whose source was left unset, even
/// though an unset source means the person was never updated and kept its existing value — so the task's
/// record of the outcome disagreed with the record it describes.
///
/// A source was only ever left unset where the merge page didn't offer a choice, and it offered one exactly
/// when the two values differed. So wherever the request and the pre-merge snapshot agree, the resolved value
/// should be the snapshot's. Two journeys go further and never offered a choice for some attributes at all, so
/// those always come from the snapshot: Teachers' Pensions for the middle name, and NPQ for all three names.
///
/// <c>SelectedPersonAttributes</c> is that pre-merge snapshot, which makes the correct value recoverable.
/// Tasks resolved by creating a new record have no snapshot and are left alone.
///
/// The closing event's payload embeds the same task data, so it's corrected alongside the task itself.
///
/// The NPQ journey's UI was removed in #3434, so those tasks are historical only — they're covered because the
/// data it left behind has the same defect, not because anything still writes it.
/// </summary>
public class BackfillResolvedAttributesJob(TrsDbContext dbContext)
{
    // Legacy events that carry the support task's before/after state, i.e. can represent a close.
    private static readonly string[] _closingLegacyEventNames =
        [.. LegacyEventBase.GetEventNamesForBaseType(typeof(LegacySupportTaskUpdatedEvent))];

    private static readonly SupportTaskType[] _supportTaskTypes =
        [SupportTaskType.TrnRequest, SupportTaskType.NpqTrnRequest, SupportTaskType.TeacherPensionsPotentialDuplicate];

    public async Task ExecuteAsync(bool dryRun, CancellationToken cancellationToken)
    {
        dbContext.Database.SetCommandTimeout(0);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var supportTasks = await dbContext.SupportTasks
            .IgnoreQueryFilters()
            .Include(t => t.TrnRequestMetadata)
            .Where(t => _supportTaskTypes.Contains(t.SupportTaskType))
            .Where(t => t.Status == SupportTaskStatus.Closed)
            .ToListAsync(cancellationToken);

        // The corrected data for each task we actually changed, keyed by reference, so the closing event's
        // copy can be brought in line with it.
        var correctedData = new Dictionary<string, ISupportTaskData>();

        foreach (var supportTask in supportTasks)
        {
            if (GetCorrectedData(supportTask) is { } corrected)
            {
                supportTask.Data = corrected;
                correctedData.Add(supportTask.SupportTaskReference, corrected);
            }
        }

        await CorrectClosingEventsAsync(correctedData, cancellationToken);

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

    /// Returns the task's data with corrected resolved attributes, or null if it needs no correction.
    private static ISupportTaskData? GetCorrectedData(SupportTask supportTask)
    {
        var requestData = supportTask.TrnRequestMetadata;

        if (requestData is null)
        {
            return null;
        }

        return supportTask.SupportTaskType switch
        {
            SupportTaskType.TrnRequest => GetCorrectedTrnRequestData(supportTask.GetData<TrnRequestData>(), requestData),
            SupportTaskType.NpqTrnRequest => GetCorrectedNpqTrnRequestData(supportTask.GetData<NpqTrnRequestData>(), requestData),
            SupportTaskType.TeacherPensionsPotentialDuplicate =>
                GetCorrectedTeacherPensionsData(supportTask.GetData<TeacherPensionsPotentialDuplicateData>(), requestData),
            _ => null
        };
    }

    private static ISupportTaskData? GetCorrectedNpqTrnRequestData(NpqTrnRequestData data, TrnRequestMetadata requestData)
    {
        // No snapshot means the request was resolved with a newly created record (or the task was rejected),
        // neither of which resolves attributes against an existing record.
        if (data is not { ResolvedAttributes: { } resolved, SelectedPersonAttributes: { } selected })
        {
            return null;
        }

        var corrected = resolved with
        {
            // This journey offered no name choices at all, so the existing record's names were always kept.
            FirstName = selected.FirstName,
            MiddleName = selected.MiddleName,
            LastName = selected.LastName,
            DateOfBirth = selected.DateOfBirth != requestData.DateOfBirth ? resolved.DateOfBirth : selected.DateOfBirth,
            EmailAddress = DifferAllowingNullOrEmpty(selected.EmailAddress, requestData.EmailAddress) ? resolved.EmailAddress : selected.EmailAddress,
            NationalInsuranceNumber = DifferAllowingNullOrEmpty(selected.NationalInsuranceNumber, requestData.NationalInsuranceNumber) ? resolved.NationalInsuranceNumber : selected.NationalInsuranceNumber,
            Gender = selected.Gender != requestData.Gender ? resolved.Gender : selected.Gender
        };

        return corrected != resolved ? data with { ResolvedAttributes = corrected } : null;
    }

    private static ISupportTaskData? GetCorrectedTrnRequestData(TrnRequestData data, TrnRequestMetadata requestData)
    {
        // No snapshot means the request was resolved with a newly created record, which takes every value
        // from the request and so was never affected.
        if (data is not { ResolvedAttributes: { } resolved, SelectedPersonAttributes: { } selected })
        {
            return null;
        }

        var corrected = resolved with
        {
            FirstName = Differ(selected.FirstName, requestData.FirstName) ? resolved.FirstName : selected.FirstName,
            MiddleName = DifferAllowingNullOrEmpty(selected.MiddleName, requestData.MiddleName) ? resolved.MiddleName : selected.MiddleName,
            LastName = Differ(selected.LastName, requestData.LastName) ? resolved.LastName : selected.LastName,
            DateOfBirth = selected.DateOfBirth != requestData.DateOfBirth ? resolved.DateOfBirth : selected.DateOfBirth,
            EmailAddress = DifferAllowingNullOrEmpty(selected.EmailAddress, requestData.EmailAddress) ? resolved.EmailAddress : selected.EmailAddress,
            NationalInsuranceNumber = DifferAllowingNullOrEmpty(selected.NationalInsuranceNumber, requestData.NationalInsuranceNumber) ? resolved.NationalInsuranceNumber : selected.NationalInsuranceNumber,
            Gender = selected.Gender != requestData.Gender ? resolved.Gender : selected.Gender
        };

        return corrected != resolved ? data with { ResolvedAttributes = corrected } : null;
    }

    private static ISupportTaskData? GetCorrectedTeacherPensionsData(
        TeacherPensionsPotentialDuplicateData data,
        TrnRequestMetadata requestData)
    {
        // No snapshot means the task was resolved without a merge, which records no resolved attributes.
        if (data is not { ResolvedAttributes: { } resolved, SelectedPersonAttributes: { } selected })
        {
            return null;
        }

        var corrected = resolved with
        {
            FirstName = Differ(selected.FirstName, requestData.FirstName) ? resolved.FirstName : selected.FirstName,
            // This journey's merge page offers no middle name choice, so the source is always unset.
            MiddleName = selected.MiddleName,
            LastName = Differ(selected.LastName, requestData.LastName) ? resolved.LastName : selected.LastName,
            DateOfBirth = selected.DateOfBirth != requestData.DateOfBirth ? resolved.DateOfBirth : selected.DateOfBirth,
            NationalInsuranceNumber = DifferAllowingNullOrEmpty(selected.NationalInsuranceNumber, requestData.NationalInsuranceNumber) ? resolved.NationalInsuranceNumber : selected.NationalInsuranceNumber,
            Gender = selected.Gender != requestData.Gender ? resolved.Gender : selected.Gender
        };

        return corrected != resolved ? data with { ResolvedAttributes = corrected } : null;
    }

    // The merge pages compare most values exactly...
    private static bool Differ(string? existingValue, string? requestValue) => existingValue != requestValue;

    // ...but treat null and empty as the same value for those a request can omit.
    private static bool DifferAllowingNullOrEmpty(string? existingValue, string? requestValue) =>
        !(existingValue == requestValue || (string.IsNullOrEmpty(existingValue) && string.IsNullOrEmpty(requestValue)));

    private async Task CorrectClosingEventsAsync(
        IReadOnlyDictionary<string, ISupportTaskData> correctedData,
        CancellationToken cancellationToken)
    {
        if (correctedData.Count == 0)
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
                correctedData.TryGetValue(payload.SupportTask.SupportTaskReference, out var corrected) &&
                ClosesTask(payload.OldSupportTask.Status, payload.SupportTask.Status))
            {
                dbContext.Entry(processEvent).Property(e => e.Payload).CurrentValue = payload with
                {
                    SupportTask = payload.SupportTask with { Data = corrected }
                };
            }
        }

        // Legacy events are stored as JSON, so edit the payload in place rather than round-tripping it
        // through the typed model.
        var legacyEvents = await dbContext.Events
            .Where(e => _closingLegacyEventNames.Contains(e.EventName))
            .ToListAsync(cancellationToken);

        foreach (var legacyEvent in legacyEvents)
        {
            if (legacyEvent.ToEventBase() is not LegacySupportTaskUpdatedEvent { SupportTask: var newTask, OldSupportTask: var oldTask } ||
                !ClosesTask(oldTask.Status, newTask.Status) ||
                !correctedData.TryGetValue(newTask.SupportTaskReference, out var corrected))
            {
                continue;
            }

            var payload = JsonNode.Parse(legacyEvent.Payload)!;

            if (payload["SupportTask"]?["Data"] is not JsonObject dataNode)
            {
                continue;
            }

            dataNode["ResolvedAttributes"] = JsonSerializer.SerializeToNode(
                GetResolvedAttributes(corrected),
                LegacyEventBase.JsonSerializerOptions);

            dbContext.Entry(legacyEvent).Property(e => e.Payload).CurrentValue = payload.ToJsonString();
        }
    }

    private static object? GetResolvedAttributes(ISupportTaskData data) => data switch
    {
        TrnRequestData d => d.ResolvedAttributes,
        NpqTrnRequestData d => d.ResolvedAttributes,
        TeacherPensionsPotentialDuplicateData d => d.ResolvedAttributes,
        _ => null
    };

    private static bool ClosesTask(SupportTaskStatus oldStatus, SupportTaskStatus newStatus) =>
        oldStatus is not SupportTaskStatus.Closed && newStatus is SupportTaskStatus.Closed;
}
