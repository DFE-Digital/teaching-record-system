using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.AddMq;

public class IndexModel : PageModel
{
    private readonly TrsLinkGenerator _linkGenerator;

    public IndexModel(TrsLinkGenerator linkGenerator)
    {
        _linkGenerator = linkGenerator;
    }

    [FromQuery]
    public Guid PersonId { get; set; }

    public IActionResult OnGet()
    {
        return Redirect(_linkGenerator.MqAddProvider(PersonId, null));
    }
}
