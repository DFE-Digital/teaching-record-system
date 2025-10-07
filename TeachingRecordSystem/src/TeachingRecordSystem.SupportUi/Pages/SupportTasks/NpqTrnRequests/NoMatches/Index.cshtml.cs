using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.NoMatches;

public class IndexModel(TrsLinkGenerator linkGenerator) : PageModel
{
    [FromRoute]
    public required string SupportTaskReference { get; init; }

    public IActionResult OnGet() => Redirect(linkGenerator.NpqTrnRequestNoMatchesCheckAnswers(SupportTaskReference));
}
