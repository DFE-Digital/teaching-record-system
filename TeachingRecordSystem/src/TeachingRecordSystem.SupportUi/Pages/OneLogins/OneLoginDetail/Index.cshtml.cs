using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.OneLogins.OneLoginDetail;

[TypeFilter(typeof(CheckOneLoginUserExistsFilterFactory))]
public class IndexModel : PageModel
{
    [FromRoute]
    public string OneLoginUserSubject { get; set; } = null!;

    public void OnGet()
    {
    }
}
