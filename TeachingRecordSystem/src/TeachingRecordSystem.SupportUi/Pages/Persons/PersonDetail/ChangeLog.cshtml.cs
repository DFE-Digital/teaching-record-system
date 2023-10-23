using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

public class ChangeLogModel : PageModel
{
    private readonly ICrmQueryDispatcher _crmQueryDispatcher;
    private readonly IClock _clock;

    public ChangeLogModel(
        ICrmQueryDispatcher crmQueryDispatcher,
        IClock clock)
    {
        _crmQueryDispatcher = crmQueryDispatcher;
        _clock = clock;
    }

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
        var contactDetail = await _crmQueryDispatcher.ExecuteQuery(
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

        var notesResult = await _crmQueryDispatcher.ExecuteQuery(new GetNotesByContactIdQuery(PersonId));
        TimelineItems = MapNotesResult(notesResult);

        return Page();
    }

    private TimelineItem[] MapNotesResult(TeacherNotesResult notesResult)
    {
        var timelineItems = new List<TimelineItem>();
        timelineItems.AddRange(notesResult.Annotations.Select(MapAnnotation));
        timelineItems.AddRange(notesResult.Tasks.Select(MapCrmTask));
        if (notesResult.IncidentResolutions != null && notesResult.IncidentResolutions.Length > 0)
        {
            timelineItems.AddRange(notesResult.IncidentResolutions.Select(MapIncidentResolution));
        }

        return timelineItems.OrderByDescending(i => i.Time).ToArray();
    }

    private TimelineItem MapAnnotation(Annotation annotation)
    {
        var user = annotation.Extract<SystemUser>(SystemUser.EntityLogicalName, SystemUser.PrimaryIdAttribute);

        return new TimelineItem
        {
            Title = "Note modified",
            User = $"{user.FirstName} {user.LastName}",
            Time = annotation.ModifiedOn.WithDqtBstFix(isLocalTime: true)!.Value,
            Summary = annotation.Subject,
            Description = annotation.NoteText,
            Status = null
        };
    }

    private TimelineItem MapCrmTask(CrmTask crmTask)
    {
        var user = crmTask.Extract<SystemUser>(SystemUser.EntityLogicalName, SystemUser.PrimaryIdAttribute);

        var titleAction = crmTask.StateCode == TaskState.Completed ? "completed" : crmTask.StateCode == TaskState.Canceled ? "cancelled" : "modified";

        return new TimelineItem
        {
            Title = $"Task {titleAction}",
            User = $"{user.FirstName} {user.LastName}",
            Time = crmTask.ModifiedOn.WithDqtBstFix(isLocalTime: true)!.Value,
            Summary = crmTask.Subject,
            Description = crmTask.Description,
            Status = crmTask.ScheduledEnd.HasValue && crmTask.ScheduledEnd.Value < _clock.UtcNow ? TimelineItemStatus.Overdue : crmTask.StateCode == TaskState.Open ? TimelineItemStatus.Active : TimelineItemStatus.Closed
        };
    }

    private TimelineItem MapIncidentResolution((IncidentResolution IncidentResolution, Incident Incident) incidentResolution)
    {
        var createdByUser = incidentResolution.IncidentResolution.Extract<SystemUser>($"{SystemUser.EntityLogicalName}_createdby", SystemUser.PrimaryIdAttribute);
        var modifiedByUser = incidentResolution.IncidentResolution.Extract<SystemUser>($"{SystemUser.EntityLogicalName}_modifiedby", SystemUser.PrimaryIdAttribute);

        string? description = null;
        var titleAction = "resolved";
        if (incidentResolution.IncidentResolution.StateCode == IncidentResolutionState.Canceled)
        {
            titleAction = "re-activated";
            description = $"Originally resolved by {createdByUser.FirstName} {createdByUser.LastName}";
        }

        return new TimelineItem
        {
            Title = $"{incidentResolution.Incident.Title} case {titleAction}",
            User = $"{modifiedByUser.FirstName} {modifiedByUser.LastName}",
            Time = incidentResolution.IncidentResolution.ModifiedOn.WithDqtBstFix(isLocalTime: true)!.Value,
            Summary = incidentResolution.IncidentResolution.Subject,
            Description = description,
            Status = null
        };
    }

    public record TimelineItem
    {
        public required string Title { get; init; }
        public required string User { get; init; }
        public required DateTime Time { get; init; }
        public required string Summary { get; init; }
        public required string? Description { get; init; }
        public required TimelineItemStatus? Status { get; init; }
    }

    public enum TimelineItemStatus
    {
        Active,
        Closed,
        Overdue
    }
}
