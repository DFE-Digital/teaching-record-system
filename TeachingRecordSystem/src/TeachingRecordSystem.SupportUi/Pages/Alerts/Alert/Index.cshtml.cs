using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.Alert;

public class IndexModel : PageModel
{
    [FromRoute]
    public Guid AlertId { get; set; }
}
