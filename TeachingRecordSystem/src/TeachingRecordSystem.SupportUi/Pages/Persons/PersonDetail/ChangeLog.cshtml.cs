using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Events;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.Timeline.Events;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

public class ChangeLogModel(ICrmQueryDispatcher crmQueryDispatcher, IDbContextFactory<TrsDbContext> dbContextFactory) : PageModel
{
    [FromRoute]
    public Guid PersonId { get; set; }

    [FromQuery]
    public string? Search { get; set; }

    [FromQuery]
    public int? PageNumber { get; set; }

    [FromQuery]
    public ContactSearchSortByOption? SortBy { get; set; }

    public string? Name { get; set; }

    public TimelineItem[]? TimelineItems { get; set; }

    public async Task<IActionResult> OnGet()
    {
        var contactDetail = await crmQueryDispatcher.ExecuteQuery(
            new GetContactDetailByIdQuery(
                PersonId,
                new ColumnSet(
                    Contact.Fields.FirstName,
                    Contact.Fields.MiddleName,
                    Contact.Fields.LastName,
                    Contact.Fields.dfeta_StatedFirstName,
                    Contact.Fields.dfeta_StatedMiddleName,
                    Contact.Fields.dfeta_StatedLastName)));

        Name = contactDetail!.Contact.ResolveFullName(includeMiddleName: false);

        var notesResult = await crmQueryDispatcher.ExecuteQuery(new GetNotesByContactIdQuery(PersonId));

        using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var personIdString = PersonId.ToString();
        var eventsWithUser = await dbContext.Database
            .SqlQuery<EventWithUser>($"""
                SELECT
                    e.event_name,
                    e.payload as event_payload,
                    u.user_id as trs_user_user_id,
                    u.active as trs_user_active,
                    u.user_type as trs_user_user_type,
                    u.name as trs_user_name,
                    u.email as trs_user_email,
                    u.azure_ad_user_id as trs_user_azure_ad_user_id,
                    u.roles as trs_user_roles,
                    u.dqt_user_id as trs_user_dqt_user_id,
                    CASE
                        WHEN e.payload #>> Array['RaisedBy','DqtUserId'] is not null THEN
                            (e.payload #>> Array['RaisedBy','DqtUserId'])::uuid
                        ELSE
                            null
                    END as dqt_user_id,
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
                    e.payload ->> 'PersonId' = {personIdString}
                    AND e.event_name in ('MandatoryQualificationDeletedEvent', 'MandatoryQualificationDqtDeactivatedEvent')
                """)
            .ToListAsync();

        TimelineItems = notesResult
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

        return Page();
    }

    private TimelineItem MapTimelineEvent(EventWithUser eventWithUser)
    {
        var @event = EventBase.Deserialize(eventWithUser.EventPayload, eventWithUser.EventName);
        User? user = null;
        if (eventWithUser.TrsUserUserId is not null)
        {
            user = new User
            {
                UserId = eventWithUser.TrsUserUserId.Value,
                Active = eventWithUser.TrsUserActive!.Value,
                UserType = eventWithUser.TrsUserUserType!.Value,
                Name = eventWithUser.TrsUserName!,
                Email = eventWithUser.TrsUserEmail,
                AzureAdUserId = eventWithUser.TrsUserAzureAdUserId,
                Roles = eventWithUser.TrsUserRoles!,
                DqtUserId = eventWithUser.TrsUserDqtUserId
            };
        }

        DqtUser? dqtUser = null;
        if (eventWithUser.DqtUserId is not null)
        {
            dqtUser = new DqtUser
            {
                UserId = eventWithUser.DqtUserId.Value,
                Name = eventWithUser.DqtUserName!
            };
        }

        RaisedByUser raiseByUser = new()
        {
            User = user,
            DqtUser = dqtUser
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
        public required Guid? TrsUserUserId { get; init; }
        public required bool? TrsUserActive { get; set; }
        public required UserType? TrsUserUserType { get; init; }
        public required string? TrsUserName { get; set; }
        public required string? TrsUserEmail { get; set; }
        public required string? TrsUserAzureAdUserId { get; set; }
        public required string[]? TrsUserRoles { get; set; }
        public required Guid? TrsUserDqtUserId { get; set; }
        public required Guid? DqtUserId { get; set; }
        public required string? DqtUserName { get; set; }
    }
}
