using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.Cases.EditCase;

[Authorize(Policy = AuthorizationPolicies.CaseManagement)]
public class AcceptModel : PageModel
{
    [FromRoute]
    public string TicketNumber { get; set; } = null!;

    public void OnGet()
    {
    }
}
