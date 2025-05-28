using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.Timeline.Events;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

public class ChangeHistoryModel(
    ICrmQueryDispatcher crmQueryDispatcher,
    TrsDbContext dbContext,
    ReferenceDataCache referenceDataCache,
    IAuthorizationService authorizationService,
    TrsLinkGenerator linkGenerator,
    IFeatureProvider featureProvider) : PageModel
{
    private const int PageSize = 10;

    [FromRoute]
    public Guid PersonId { get; set; }

    [FromQuery]
    public int? PageNumber { get; set; }

    [FromQuery]
    public ContactSearchSortByOption? SortBy { get; set; }

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

        var personInfo = HttpContext.GetCurrentPersonFeature();

        var notesResult = await crmQueryDispatcher.ExecuteQueryAsync(new GetNotesByContactIdQuery(PersonId));

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
            nameof(ProfessionalStatusCreatedEvent),
            nameof(ProfessionalStatusUpdatedEvent),
            nameof(ProfessionalStatusDeletedEvent)
        };

        var alertEventTypes = eventTypes.Where(et => et.StartsWith("Alert")).ToArray();

        var alertTypesWithReadPermission = await referenceDataCache.GetAlertTypesAsync(activeOnly: false)
            .ToAsyncEnumerableAsync()
            .SelectAwait(async at => (
                AlertType: at,
                CanRead: (await authorizationService.AuthorizeForAlertTypeAsync(User, at.AlertTypeId, Permissions.Alerts.Read)) is { Succeeded: true }))
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
                    e.payload #>> Array['RaisedBy','DqtUserName'] as dqt_user_name
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
                WHERE
                    e.person_id = {PersonId}
                    AND e.event_name = any ({eventTypes})

                    -- Only return alerts that have an alert type (or DQT sanction code) that the user is authorized to Read
                    AND (
                        NOT (e.event_name = any({alertEventTypes}))
                        OR (e.payload #>> Array['Alert','AlertTypeId'])::uuid = any({alertTypeIdsWithReadPermission})
                        OR (e.payload #>> Array['Alert','DqtSanctionCode','Value']) = any({dqtSanctionCodesWithReadPermission})
                    )
                """)
            .ToListAsync();

        var allResults = notesResult
            .Annotations.Select(n => (TimelineItem)new TimelineItem<Annotation>(
                TimelineItemType.Annotation,
                n.ModifiedOn!.Value.ToLocal(),
                n))
            .Concat(notesResult.IncidentResolutions.Select(r => new TimelineItem<(IncidentResolution, Incident)>(
                TimelineItemType.IncidentResolution,
                r.Resolution.ModifiedOn!.Value.ToLocal(),
                r)))
            .Concat(notesResult.Tasks.Select(t => new TimelineItem<CrmTask>(
                TimelineItemType.Task,
                t.ModifiedOn!.Value.ToLocal(),
                t)))
            .Concat(eventsWithUser.Select(MapTimelineEvent))
            .OrderByDescending(i => i.Timestamp)
            .ToArray();

        //exclude all crm annotations if dqtnote is enabled.
        //
        //NOTE: When the backfilling of dqtnotes has been ran, adding annotations can be removed entirely from this page.
        if (featureProvider.IsEnabled(FeatureNames.DqtNotes))
        {
            allResults = allResults.Where(x => x.ItemType != TimelineItemType.Annotation).ToArray();
        }

        TimelineItems = allResults
            .Skip((PageNumber!.Value - 1) * PageSize)
            .Take(PageSize)
            .ToArray();

        // If an 'out of bounds' page was requested, redirect to the first page
        if (TimelineItems.Length == 0 && PageNumber > 1)
        {
            return Redirect(linkGenerator.PersonChangeHistory(PersonId, pageNumber: 1));
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

        RaisedByUserInfo raiseByUser = new()
        {
            Name = eventWithUser.TrsUserName ?? eventWithUser.DqtUserName!
        };

        var timelineEventType = typeof(TimelineEvent<>).MakeGenericType(@event.GetType()!);
        var timelineEvent = (TimelineEvent)Activator.CreateInstance(timelineEventType, @event, raiseByUser)!;
        var timelineItemType = typeof(TimelineItem<>).MakeGenericType(timelineEventType);
        return (TimelineItem)Activator.CreateInstance(timelineItemType, TimelineItemType.Event, timelineEvent.Event.CreatedUtc.ToLocal(), timelineEvent)!;
    }

    /// <summary>
    /// Flattened out record to allow Event, TRS User and DQT User to be returned in a single SQL query
    /// </summary>
    private record EventWithUser
    {
        public required string EventName { get; init; }
        public required string EventPayload { get; init; }
        public required string? TrsUserName { get; set; }
        public required string? DqtUserName { get; set; }
    }
}
