using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.NoMatches;

public class IndexModel(TrsLinkGenerator linkGenerator) : PageModel
{
    [FromRoute]
    public string SupportTaskReference { get; set; }

    public IActionResult OnGet() => Redirect(linkGenerator.NpqTrnRequestNoMatchesCheckAnswers(SupportTaskReference));
}
