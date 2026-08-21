using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Services.ChangeHistory;

public class ChangeHistoryService(
    TrsDbContext dbContext,
    ReferenceDataCache referenceDataCache,
    IAuthorizationService authorizationService)
{
    public async Task<ResultPage<TimelineItem>> GetChangeHistoryByPersonAsync(
        Guid personId,
        ClaimsPrincipal user,
        PaginationOptions paginationOptions)
    {
        var eventTypes = new[]
        {
            nameof(LegacyEvents.MandatoryQualificationDqtDeactivatedEvent),
            nameof(LegacyEvents.MandatoryQualificationDqtReactivatedEvent),
            nameof(LegacyEvents.MandatoryQualificationDqtImportedEvent),
            nameof(LegacyEvents.MandatoryQualificationMigratedEvent),
            nameof(LegacyEvents.DqtInductionImportedEvent),
            nameof(LegacyEvents.DqtInductionCreatedEvent),
            nameof(LegacyEvents.DqtInductionUpdatedEvent),
            nameof(LegacyEvents.InductionMigratedEvent),
            nameof(LegacyEvents.DqtInductionDeactivatedEvent),
            nameof(LegacyEvents.DqtInductionReactivatedEvent),
            nameof(LegacyEvents.DqtContactInductionStatusChangedEvent),
            nameof(LegacyEvents.PersonInductionUpdatedEvent),
            nameof(LegacyEvents.PersonDetailsUpdatedEvent),
            nameof(LegacyEvents.PersonCreatedEvent),
            nameof(LegacyEvents.RouteToProfessionalStatusCreatedEvent),
            nameof(LegacyEvents.RouteToProfessionalStatusUpdatedEvent),
            nameof(LegacyEvents.RouteToProfessionalStatusDeletedEvent),
            nameof(LegacyEvents.RouteToProfessionalStatusMigratedEvent),
            nameof(LegacyEvents.ApiTrnRequestSupportTaskUpdatedEvent),
            nameof(LegacyEvents.NpqTrnRequestSupportTaskResolvedEvent),
            nameof(LegacyEvents.DqtInitialTeacherTrainingCreatedEvent),
            nameof(LegacyEvents.DqtInitialTeacherTrainingUpdatedEvent),
            nameof(LegacyEvents.DqtQtsRegistrationCreatedEvent),
            nameof(LegacyEvents.DqtQtsRegistrationUpdatedEvent),
            nameof(LegacyEvents.PersonStatusUpdatedEvent),
            nameof(LegacyEvents.PersonsMergedEvent),
            nameof(LegacyEvents.TrnAllocatedEvent),
            nameof(LegacyEvents.TeacherPensionsPotentialDuplicateSupportTaskResolvedEvent),
            nameof(LegacyEvents.ChangeNameRequestSupportTaskApprovedEvent),
            nameof(LegacyEvents.ChangeNameRequestSupportTaskRejectedEvent),
            nameof(LegacyEvents.ChangeDateOfBirthRequestSupportTaskApprovedEvent),
            nameof(LegacyEvents.ChangeDateOfBirthRequestSupportTaskRejectedEvent),
            nameof(OneLoginUserUpdatedEvent)
        };

        var alertTypesWithReadPermission = await referenceDataCache.GetAlertTypesAsync(activeOnly: false)
            .ToAsyncEnumerableAsync()
            .Select(async (AlertType at, CancellationToken _) => (
                AlertType: at,
                CanRead: (await authorizationService.AuthorizeAsync(user, at.AlertTypeId, new AlertTypePermissionRequirement(Permissions.Alerts.Read))) is { Succeeded: true }))
            .Where(t => t.CanRead)
            .ToArrayAsync();

        var alertTypeIdsWithReadPermission = alertTypesWithReadPermission.Select(at => at.AlertType.AlertTypeId).ToArray();

        var dqtSanctionCodesWithReadPermission = alertTypesWithReadPermission
            .Select(at => at.AlertType.DqtSanctionCode)
            .Where(sc => sc is not null)
            .ToArray();

        var eventsWithUser = await dbContext.Database
            .SqlQuery<EventWithUser>($"""
                SELECT
                    e.event_name,
                    e.payload as event_payload,
                    u.name as trs_user_name,
                    e.payload #>> ARRAY['RaisedBy','DqtUserName'] as dqt_user_name,
                    a.name as application_user_name,
                    a.short_name as application_user_short_name
                FROM
                        events as e
                    LEFT JOIN
                        users as u ON
                            CASE
                                WHEN e.payload #>> ARRAY['RaisedBy','DqtUserId'] is null THEN
                                    (e.payload ->> 'RaisedBy')::uuid
                                ELSE
                                    null
                            END = u.user_id
                    LEFT JOIN
                        users as a ON ((e.payload #>> ARRAY['RequestData','ApplicationUserId']) :: uuid) = a.user_id
                WHERE
                    e.person_ids @> ARRAY[{personId}]
                    AND e.event_name = any({eventTypes})

                    -- Only return TPS resolved duplicate events that are merges where the imported record has not been kept
                    AND
                    (
                        e.event_name <> {nameof(LegacyEvents.TeacherPensionsPotentialDuplicateSupportTaskResolvedEvent)}
                        OR (e.payload->> 'ChangeReason')::int != {LegacyEvents.TeacherPensionsPotentialDuplicateSupportTaskResolvedReason.RecordKept}
                    )
                """)
            .ToListAsync();

        var processTypesToQuery = new[]
        {
            ProcessType.PersonCreatingInDqt,
            ProcessType.PersonImportingIntoDqt,
            ProcessType.PersonUpdatingInDqt,
            ProcessType.PersonDeactivatingInDqt,
            ProcessType.PersonReactivatingInDqt,
            ProcessType.PersonMergingInDqt,
            ProcessType.AlertCreating,
            ProcessType.AlertUpdating,
            ProcessType.AlertDeleting,
            ProcessType.AlertDeactivatingInDqt,
            ProcessType.AlertImportingIntoDqt,
            ProcessType.AlertReactivatingInDqt,
            ProcessType.AlertMigratingFromDqt,
            ProcessType.MandatoryQualificationCreating,
            ProcessType.MandatoryQualificationUpdating,
            ProcessType.MandatoryQualificationDeleting,
            ProcessType.PersonOneLoginUserDisconnecting,
            ProcessType.PersonOneLoginUserConnecting,
            ProcessType.OneLoginUserRecordMatchingSupportTaskCompleting,
            ProcessType.OneLoginUserIdVerificationSupportTaskCompleting,
            ProcessType.OneLoginUserPersonConnecting,
            ProcessType.OneLoginUserPersonDisconnecting
        };

        var processes = await dbContext.Processes
            .Where(p => p.PersonIds.Contains(personId) && processTypesToQuery.Contains(p.ProcessType))
            .Include(p => p.User)
            .Include(p => p.Events).AsSplitQuery()
            .ToListAsync();

        // Filter alert processes by alert type permissions
        var alertProcessTypes = new[]
        {
            ProcessType.AlertCreating,
            ProcessType.AlertUpdating,
            ProcessType.AlertDeleting,
            ProcessType.AlertDeactivatingInDqt,
            ProcessType.AlertImportingIntoDqt,
            ProcessType.AlertReactivatingInDqt,
            ProcessType.AlertMigratingFromDqt
        };
        var filteredProcesses = processes.Where(p =>
        {
            if (!alertProcessTypes.Contains(p.ProcessType))
            {
                return true;
            }

            var alertEvent = p.Events!.First(e => e.Payload is
                AlertCreatedEvent or AlertUpdatedEvent or AlertDeletedEvent or
                AlertDqtDeactivatedEvent or AlertDqtImportedEvent or AlertDqtReactivatedEvent or AlertMigratedEvent);
            (Guid? alertTypeId, EventModels.AlertDqtSanctionCode? dqtSanctionCode) = alertEvent.Payload switch
            {
                AlertCreatedEvent created => (created.Alert.AlertTypeId, created.Alert.DqtSanctionCode),
                AlertUpdatedEvent updated => (updated.Alert.AlertTypeId, updated.Alert.DqtSanctionCode),
                AlertDeletedEvent deleted => (deleted.Alert.AlertTypeId, deleted.Alert.DqtSanctionCode),
                AlertDqtDeactivatedEvent deactivated => (deactivated.Alert.AlertTypeId, deactivated.Alert.DqtSanctionCode),
                AlertDqtImportedEvent imported => (imported.Alert.AlertTypeId, imported.Alert.DqtSanctionCode),
                AlertDqtReactivatedEvent reactivated => (reactivated.Alert.AlertTypeId, reactivated.Alert.DqtSanctionCode),
                AlertMigratedEvent migrated => (migrated.Alert.AlertTypeId, migrated.Alert.DqtSanctionCode),
                _ => (null, null)
            };

            return (alertTypeId.HasValue && alertTypeIdsWithReadPermission.Contains(alertTypeId.Value))
                || (dqtSanctionCode is not null && dqtSanctionCodesWithReadPermission.Contains(dqtSanctionCode.Value));
        }).ToList();

        var allResults = eventsWithUser.Select(e => MapLegacyEvent(e, personId))
            .Concat(filteredProcesses.Select(p => MapProcess(p, personId)))
            .ToArray();

        var pageNumber = paginationOptions.PageNumber ?? 1;

        var items = allResults
            .OrderByDescending(i => i.Timestamp)
            .Skip((pageNumber - 1) * paginationOptions.PageSize)
            .Take(paginationOptions.PageSize)
            .ToArray();

        return new ResultPage<TimelineItem>(items, pageNumber, paginationOptions.PageSize, allResults.Length);
    }

    public async Task<IReadOnlyCollection<ProcessChangeHistoryEntry>> GetChangeHistoryBySupportTaskAsync(
        string supportTaskReference)
    {
        return await dbContext.Processes
            .Where(p => p.SupportTaskReferences.Contains(supportTaskReference))
            .Include(p => p.User)
            .Include(p => p.Events).AsSplitQuery()
            .OrderByDescending(p => p.CreatedOn)
            .Select(process =>
                new ProcessChangeHistoryEntry(
                    process,
                    new RaisedByUserInfo { Name = process.User != null ? process.User.Name : process.DqtUserName! }))
            .ToArrayAsync();
    }

    public async Task<ResultPage<ProcessChangeHistoryEntry>> GetChangeHistoryByOneLoginUserAsync(
        string oneLoginUserSubject,
        PaginationOptions paginationOptions)
    {
        var processTypesToQuery = new[]
        {
            ProcessType.PersonOneLoginUserConnecting,
            ProcessType.PersonOneLoginUserDisconnecting,
            ProcessType.OneLoginUserRecordMatchingSupportTaskCompleting,
            ProcessType.OneLoginUserIdVerificationSupportTaskCompleting,
            ProcessType.OneLoginUserPersonConnecting,
            ProcessType.OneLoginUserPersonDisconnecting
        };

        var query = dbContext.Processes
            .Where(p => p.OneLoginUserSubjects.Contains(oneLoginUserSubject) && processTypesToQuery.Contains(p.ProcessType))
            .Include(p => p.User)
            .Include(p => p.Events).AsSplitQuery()
            .OrderByDescending(p => p.CreatedOn);

        var totalCount = await query.CountAsync();

        return await query
            .Select(process =>
                new ProcessChangeHistoryEntry(
                    process,
                    new RaisedByUserInfo { Name = process.User != null ? process.User.Name : process.DqtUserName! }))
            .GetPageAsync(paginationOptions.PageNumber, paginationOptions.PageSize, totalCount);
    }

    private TimelineItem MapLegacyEvent(EventWithUser eventWithUser, Guid personId)
    {
        var @event = LegacyEvents.EventBase.Deserialize(eventWithUser.EventPayload, eventWithUser.EventName);

        var raisedByUser = new RaisedByUserInfo
        {
            Name = eventWithUser.TrsUserName ?? eventWithUser.DqtUserName!
        };

        ApplicationUserInfo? applicationUser = eventWithUser.ApplicationUserName == null ? null : new()
        {
            Name = eventWithUser.ApplicationUserName,
            ShortName = eventWithUser.ApplicationUserShortName ?? eventWithUser.ApplicationUserName
        };

        var timelineEventType = typeof(LegacyEventChangeHistoryEntry<>).MakeGenericType(@event.GetType());
        var timelineEvent = (LegacyEventChangeHistoryEntry)Activator.CreateInstance(timelineEventType, @event, raisedByUser, applicationUser)!;
        var timelineItemType = typeof(TimelineItem<>).MakeGenericType(timelineEventType);
        return (TimelineItem)Activator.CreateInstance(timelineItemType, TimelineItemType.LegacyEvent, personId, timelineEvent.Event.CreatedUtc, timelineEvent)!;
    }

    private TimelineItem MapProcess(Process process, Guid personId) =>
        new TimelineItem<ProcessChangeHistoryEntry>(
            TimelineItemType.Process,
            personId,
            process.CreatedOn,
            new ProcessChangeHistoryEntry(process, new RaisedByUserInfo { Name = process.DqtUserName ?? process.User?.Name! }));

    private record EventWithUser
    {
        public required string EventName { get; init; }
        public required string EventPayload { get; init; }
        public required string? TrsUserName { get; init; }
        public required string? DqtUserName { get; init; }
        public required string? ApplicationUserName { get; init; }
        public required string? ApplicationUserShortName { get; init; }
    }
}
