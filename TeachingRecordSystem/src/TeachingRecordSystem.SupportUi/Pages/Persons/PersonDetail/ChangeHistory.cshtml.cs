using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.Timeline.Events;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

[AllowDeactivatedPerson]
public class ChangeHistoryModel(
    TrsDbContext dbContext,
    ReferenceDataCache referenceDataCache,
    IAuthorizationService authorizationService,
    SupportUiLinkGenerator linkGenerator) : PageModel
{
    private const int PageSize = 10;

    [FromRoute]
    public Guid PersonId { get; set; }

    [FromQuery]
    public int? PageNumber { get; set; }

    [FromQuery]
    public PersonSearchSortByOption? SortBy { get; set; }

    public TimelineItem[]? TimelineItems { get; set; }

    public int[]? PaginationPages { get; set; }

    public bool GotPreviousPage { get; set; }

    public bool GotNextPage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        PageNumber ??= 1;

        if (PageNumber < 1)
        {
            return BadRequest();
        }

        var eventTypes = new[]
        {
            nameof(MandatoryQualificationDeletedEvent),
            nameof(MandatoryQualificationDqtDeactivatedEvent),
            nameof(MandatoryQualificationUpdatedEvent),
            nameof(MandatoryQualificationDqtReactivatedEvent),
            nameof(MandatoryQualificationCreatedEvent),
            nameof(MandatoryQualificationDqtImportedEvent),
            nameof(MandatoryQualificationMigratedEvent),
            nameof(AlertCreatedEvent),
            nameof(AlertUpdatedEvent),
            nameof(AlertDeletedEvent),
            nameof(AlertMigratedEvent),
            nameof(AlertDqtDeactivatedEvent),
            nameof(AlertDqtImportedEvent),
            nameof(AlertDqtReactivatedEvent),
            nameof(DqtInductionImportedEvent),
            nameof(DqtInductionCreatedEvent),
            nameof(DqtInductionUpdatedEvent),
            nameof(InductionMigratedEvent),
            nameof(DqtInductionDeactivatedEvent),
            nameof(DqtInductionReactivatedEvent),
            nameof(DqtContactInductionStatusChangedEvent),
            nameof(PersonInductionUpdatedEvent),
            nameof(PersonDetailsUpdatedEvent),
            nameof(PersonCreatedEvent),
            nameof(RouteToProfessionalStatusCreatedEvent),
            nameof(RouteToProfessionalStatusUpdatedEvent),
            nameof(RouteToProfessionalStatusDeletedEvent),
            nameof(RouteToProfessionalStatusMigratedEvent),
            nameof(ApiTrnRequestSupportTaskUpdatedEvent),
            nameof(NpqTrnRequestSupportTaskResolvedEvent),
            nameof(DqtInitialTeacherTrainingCreatedEvent),
            nameof(DqtInitialTeacherTrainingUpdatedEvent),
            nameof(DqtQtsRegistrationCreatedEvent),
            nameof(DqtQtsRegistrationUpdatedEvent),
            nameof(PersonStatusUpdatedEvent),
            nameof(PersonsMergedEvent),
            nameof(TrnAllocatedEvent),
            nameof(TeacherPensionsPotentialDuplicateSupportTaskResolvedEvent)
        };

        var alertEventTypes = eventTypes.Where(et => et.StartsWith("Alert", StringComparison.Ordinal)).ToArray();

        var alertTypesWithReadPermission = await referenceDataCache.GetAlertTypesAsync(activeOnly: false)
            .ToAsyncEnumerableAsync()
            .SelectAwait(async at => (
                AlertType: at,
                CanRead: (await authorizationService.AuthorizeAsync(User, at.AlertTypeId, new AlertTypePermissionRequirement(Permissions.Alerts.Read))) is { Succeeded: true }))
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
                    e.payload #>> Array['RaisedBy','DqtUserName'] as dqt_user_name,
                    a.name as application_user_name,
                    a.short_name as application_user_short_name
                FROM
                        events as e
                    LEFT JOIN
                        users as u ON
                            CASE
                                WHEN e.payload #>> Array['RaisedBy','DqtUserId'] is null THEN
                                    (e.payload ->> 'RaisedBy')::uuid
                                ELSE
                                    null
                            END = u.user_id
                    LEFT JOIN
                        users as a ON ((e.payload #>> Array['RequestData','ApplicationUserId']) :: uuid) = a.user_id
                WHERE
                    e.person_ids @> ARRAY[{PersonId}]
                    AND e.event_name = any ({eventTypes})

                    -- only tps resolved duplicate events that are merges and not where the imported record has been kept
                    AND
                    (
                        e.event_name <> {nameof(TeacherPensionsPotentialDuplicateSupportTaskResolvedEvent)}
                        OR (e.payload->> 'ChangeReason')::int != {TeacherPensionsPotentialDuplicateSupportTaskResolvedReason.RecordKept}
                    )

                    -- Only return alerts that have an alert type (or DQT sanction code) that the user is authorized to Read
                    AND (
                        NOT (e.event_name = any({alertEventTypes}))
                        OR (e.payload #>> Array['Alert','AlertTypeId'])::uuid = any({alertTypeIdsWithReadPermission})
                        OR (e.payload #>> Array['Alert','DqtSanctionCode','Value']) = any({dqtSanctionCodesWithReadPermission})
                    )
                """)
            .ToListAsync();

        var allResults = eventsWithUser.Select(MapTimelineEvent).ToArray();

        TimelineItems = allResults
            .OrderByDescending(i => i.Timestamp)
            .Skip((PageNumber!.Value - 1) * PageSize)
            .Take(PageSize)
            .ToArray();

        // If an 'out of bounds' page was requested, redirect to the first page
        if (TimelineItems.Length == 0 && PageNumber > 1)
        {
            return Redirect(linkGenerator.Persons.PersonDetail.ChangeHistory(PersonId, pageNumber: 1));
        }

        var totalPages = (int)Math.Ceiling(allResults.Length / (decimal)PageSize);
        PaginationPages = Enumerable.Range(1, totalPages).ToArray();
        GotPreviousPage = PageNumber.Value > 1;
        GotNextPage = PageNumber.Value < totalPages;

        return Page();
    }

    private TimelineItem MapTimelineEvent(EventWithUser eventWithUser)
    {
        var @event = EventBase.Deserialize(eventWithUser.EventPayload, eventWithUser.EventName);

        RaisedByUserInfo raisedByUser = new()
        {
            Name = eventWithUser.TrsUserName ?? eventWithUser.DqtUserName!
        };

        ApplicationUserInfo? applicationUser = eventWithUser.ApplicationUserName == null ? null : new()
        {
            Name = eventWithUser.ApplicationUserName,
            ShortName = eventWithUser.ApplicationUserShortName ?? eventWithUser.ApplicationUserName
        };

        var timelineEventType = typeof(TimelineEvent<>).MakeGenericType(@event.GetType()!);
        var timelineEvent = (TimelineEvent)Activator.CreateInstance(timelineEventType, @event, raisedByUser, applicationUser)!;
        var timelineItemType = typeof(TimelineItem<>).MakeGenericType(timelineEventType);
        return (TimelineItem)Activator.CreateInstance(timelineItemType, TimelineItemType.Event, PersonId, timelineEvent.Event.CreatedUtc.ToGmt(), timelineEvent)!;
    }

    /// <summary>
    /// Flattened out record to allow Event, TRS User and DQT User to be returned in a single SQL query
    /// </summary>
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
