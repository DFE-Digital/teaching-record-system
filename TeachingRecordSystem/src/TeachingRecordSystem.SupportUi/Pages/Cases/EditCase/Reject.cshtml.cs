using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.Cases.EditCase;

[Authorize(Policy = AuthorizationPolicies.CaseManagement)]
public class RejectModel : PageModel
{
    public void OnGet()
    {
    }
}
