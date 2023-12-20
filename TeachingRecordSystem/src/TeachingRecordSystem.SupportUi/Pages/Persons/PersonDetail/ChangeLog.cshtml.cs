using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

public class ChangeLogModel(ICrmQueryDispatcher crmQueryDispatcher) : PageModel
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

        TimelineItems = notesResult
            .Annotations.Select(n => (TimelineItem)new TimelineItem<Annotation>(
                TimelineItemType.Annotation,
                n.ModifiedOn.WithDqtBstFix(isLocalTime: true)!.Value,
                n))
            .Concat(notesResult.IncidentResolutions.Select(r => new TimelineItem<(IncidentResolution, Incident)>(
                TimelineItemType.IncidentResolution,
                r.Resolution.ModifiedOn.WithDqtBstFix(isLocalTime: true)!.Value,
                r)))
            .Concat(notesResult.Tasks.Select(t => new TimelineItem<CrmTask>(
                TimelineItemType.Task,
                t.ModifiedOn.WithDqtBstFix(isLocalTime: true)!.Value,
                t)))
            .OrderByDescending(i => i.Timestamp)
            .ToArray();

        return Page();
    }
}
