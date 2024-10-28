using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.Timeline.Events;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

public class ChangeHistoryModel(ICrmQueryDispatcher crmQueryDispatcher, TrsDbContext dbContext, TrsLinkGenerator linkGenerator) : PageModel
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

    public async Task<IActionResult> OnGet()
    {
        PageNumber ??= 1;

        if (PageNumber < 1)
        {
            return BadRequest();
        }

        var personInfo = HttpContext.GetCurrentPersonFeature();

        var notesResult = await crmQueryDispatcher.ExecuteQuery(new GetNotesByContactIdQuery(PersonId));

        var eventTypes = new[]
        {
            nameof(MandatoryQualificationDeletedEvent),
            nameof(MandatoryQualificationDqtDeactivatedEvent),
            nameof(MandatoryQualificationUpdatedEvent),
            nameof(MandatoryQualificationDqtReactivatedEvent),
            nameof(MandatoryQualificationCreatedEvent),
            nameof(MandatoryQualificationDqtImportedEvent),
            nameof(MandatoryQualificationMigratedEvent),
        };

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
