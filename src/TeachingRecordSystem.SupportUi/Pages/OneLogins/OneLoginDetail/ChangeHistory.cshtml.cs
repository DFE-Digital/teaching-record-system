using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;
using TeachingRecordSystem.SupportUi.Pages.Shared;
using TeachingRecordSystem.SupportUi.Pages.Shared.Components.ChangeHistoryEntry;
using TeachingRecordSystem.SupportUi.Services;
using TeachingRecordSystem.SupportUi.Services.ChangeHistory;

namespace TeachingRecordSystem.SupportUi.Pages.OneLogins.OneLoginDetail;

[TypeFilter(typeof(CheckOneLoginUserExistsFilterFactory))]
[RequireFeatureEnabledFilterFactory(FeatureNames.SupportTaskChangeHistory)]
public class ChangeHistoryModel(ChangeHistoryService changeHistoryService, SupportUiLinkGenerator linkGenerator) : PageModel
{
    private const int PageSize = 10;

    [FromRoute]
    public string OneLoginUserSubject { get; set; } = string.Empty;

    [FromQuery]
    public int? PageNumber { get; set; }

    public IReadOnlyCollection<ChangeHistoryEntryViewModel>? ChangeHistory { get; set; }

    public PaginationViewModel? Pagination { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        PageNumber ??= 1;

        if (PageNumber < 1)
        {
            return BadRequest();
        }

        var changeHistoryContext = ChangeHistoryContext.ForOneLoginUser(OneLoginUserSubject);
        var items = await changeHistoryService.GetChangeHistoryByOneLoginUserAsync(
            OneLoginUserSubject,
            new PaginationOptions(PageNumber, PageSize));

        ChangeHistory = items
            .Select(e => new ChangeHistoryEntryViewModel
            {
                Context = changeHistoryContext,
                Timestamp = e.Process.CreatedOn,
                UserName = e.RaisedByUser.Name,
                ProcessId = e.Process.ProcessId,
                ProcessType = e.Process.ProcessType,
                ChangeReason = e.Process.ChangeReason,
                Events = e.Process.Events!.Select(v => v.Payload).AsReadOnly(),
            })
            .AsReadOnly();

        // If an 'out of bounds' page was requested, redirect to the first page
        if (ChangeHistory.Count == 0 && PageNumber > 1)
        {
            return Redirect(linkGenerator.OneLogins.OneLoginDetail.ChangeHistory(OneLoginUserSubject, pageNumber: 1));
        }

        Pagination = new PaginationViewModel(
            items.CurrentPage,
            items.LastPage,
            pageNumber => linkGenerator.OneLogins.OneLoginDetail.ChangeHistory(OneLoginUserSubject, pageNumber));

        return Page();
    }
}
