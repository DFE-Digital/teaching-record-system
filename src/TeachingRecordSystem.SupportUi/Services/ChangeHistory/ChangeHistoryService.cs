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
    PersonInfoCache personInfoCache,
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
            nameof(LegacyEvents.AlertMigratedEvent),
            nameof(LegacyEvents.AlertDqtDeactivatedEvent),
            nameof(LegacyEvents.AlertDqtImportedEvent),
            nameof(LegacyEvents.AlertDqtReactivatedEvent),
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

        var alertEventTypes = eventTypes.Where(et => et.StartsWith("Alert", StringComparison.Ordinal)).ToArray();

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

                    -- Only return alerts that have an alert type (or DQT sanction code) that the user is authorized to Read
                    AND (
                        NOT (e.event_name = any({alertEventTypes}))
                        OR (e.payload #>> Array['Alert','AlertTypeId'])::uuid = any({alertTypeIdsWithReadPermission})
                        OR (e.payload #>> Array['Alert','DqtSanctionCode','Value']) = any({dqtSanctionCodesWithReadPermission})
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
            ProcessType.MandatoryQualificationCreating,
            ProcessType.MandatoryQualificationUpdating,
            ProcessType.MandatoryQualificationDeleting,
            ProcessType.PersonOneLoginUserDisconnecting,
            ProcessType.PersonOneLoginUserConnecting,
            ProcessType.OneLoginUserRecordMatchingSupportTaskCompleting,
            ProcessType.OneLoginUserIdVerificationSupportTaskCompleting,
        };

        var processes = await dbContext.Processes
            .Where(p => p.PersonIds.Contains(personId) && processTypesToQuery.Contains(p.ProcessType))
            .Include(p => p.User)
            .Include(p => p.Events).AsSplitQuery()
            .ToListAsync();

        // Filter alert processes by alert type permissions
        var alertProcessTypes = new[] { ProcessType.AlertCreating, ProcessType.AlertUpdating, ProcessType.AlertDeleting };
        var filteredProcesses = processes.Where(p =>
        {
            if (!alertProcessTypes.Contains(p.ProcessType))
            {
                return true;
            }

            var alertEvent = p.Events!.First(e => e.Payload is AlertCreatedEvent or AlertUpdatedEvent or AlertDeletedEvent);
            (Guid? alertTypeId, EventModels.AlertDqtSanctionCode? dqtSanctionCode) = alertEvent.Payload switch
            {
                AlertCreatedEvent created => (created.Alert.AlertTypeId, created.Alert.DqtSanctionCode),
                AlertUpdatedEvent updated => (updated.Alert.AlertTypeId, updated.Alert.DqtSanctionCode),
                AlertDeletedEvent deleted => (deleted.Alert.AlertTypeId, deleted.Alert.DqtSanctionCode),
                _ => (null, null)
            };

            return (alertTypeId.HasValue && alertTypeIdsWithReadPermission.Contains(alertTypeId.Value))
                || (dqtSanctionCode is not null && dqtSanctionCodesWithReadPermission.Contains(dqtSanctionCode.Value));
        }).ToList();

        var personInfo = await filteredProcesses
            .SelectMany(p => p.PersonIds)
            .Distinct()
            .ToAsyncEnumerable()
            .Select(async (Guid id, CancellationToken _) => await personInfoCache.GetPersonInfoAsync(id))
            .Where(i => i is not null)
            .ToDictionaryAsync(i => i!.PersonId, i => i!);

        var allResults = eventsWithUser.Select(e => MapLegacyEvent(e, personId))
            .Concat(filteredProcesses.Select(p => MapProcess(p, personId, personInfo)))
            .ToArray();

        var pageNumber = paginationOptions.PageNumber ?? 1;

        var items = allResults
            .OrderByDescending(i => i.Timestamp)
            .Skip((pageNumber - 1) * paginationOptions.PageSize)
            .Take(paginationOptions.PageSize)
            .ToArray();

        return new ResultPage<TimelineItem>(items, pageNumber, paginationOptions.PageSize, allResults.Length);
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

    private TimelineItem MapProcess(Process process, Guid personId, IReadOnlyDictionary<Guid, PersonInfo> personInfo) =>
        new TimelineItem<ProcessChangeHistoryEntry>(
            TimelineItemType.Process,
            personId,
            process.CreatedOn,
            new ProcessChangeHistoryEntry(process, new RaisedByUserInfo { Name = process.DqtUserName ?? process.User?.Name! }, personInfo));

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
