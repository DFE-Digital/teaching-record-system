using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.AuthorizeAccessToATeacherRecord.Pages;

[Authorize]
public class IndexModel : PageModel
{
    public void OnGet()
    {
    }
}
