using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Shared;
using TeachingRecordSystem.SupportUi.Services;
using TeachingRecordSystem.SupportUi.Services.ChangeHistory;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

[AllowDeactivatedPerson]
public class ChangeHistoryModel(ChangeHistoryService changeHistoryService, SupportUiLinkGenerator linkGenerator) : PageModel
{
    private const int PageSize = 10;

    [FromRoute]
    public Guid PersonId { get; set; }

    [FromQuery]
    public int? PageNumber { get; set; }

    public IReadOnlyCollection<TimelineItem>? TimelineItems { get; set; }

    public PaginationViewModel? Pagination { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        PageNumber ??= 1;

        if (PageNumber < 1)
        {
            return BadRequest();
        }

        var items = await changeHistoryService.GetChangeHistoryByPersonAsync(
            PersonId,
            User,
            new PaginationOptions(PageNumber, PageSize));
        TimelineItems = items;

        // If an 'out of bounds' page was requested, redirect to the first page
        if (TimelineItems.Count == 0 && PageNumber > 1)
        {
            return Redirect(linkGenerator.Persons.PersonDetail.ChangeHistory(PersonId, pageNumber: 1));
        }

        Pagination = new PaginationViewModel(
            items.CurrentPage,
            items.LastPage,
            pageNumber => linkGenerator.Persons.PersonDetail.ChangeHistory(PersonId, pageNumber));

        return Page();
    }
}
