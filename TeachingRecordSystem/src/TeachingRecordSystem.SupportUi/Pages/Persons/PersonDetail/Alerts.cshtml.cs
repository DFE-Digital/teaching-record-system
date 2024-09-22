using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

public class AlertsModel : PageModel
{
    [FromRoute]
    public Guid PersonId { get; set; }
}
