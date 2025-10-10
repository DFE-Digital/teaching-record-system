using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.NoMatches;

public class IndexModel(SupportUiLinkGenerator linkGenerator) : PageModel
{
    [FromRoute]
    public required string SupportTaskReference { get; init; }

    public IActionResult OnGet() => Redirect(linkGenerator.SupportTasks.NpqTrnRequests.NoMatches.CheckAnswers(SupportTaskReference));
}
