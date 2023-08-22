using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.Cases.EditCase;

public class IndexModel : PageModel
{
    [FromRoute]
    public string CaseId { get; set; } = null!;

    public void OnGet()
    {
    }
}
