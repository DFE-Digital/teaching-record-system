using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve;

[Journey(JourneyNames.ResolveTpsPotentialDuplicate), RequireJourneyInstance]
public class CheckAnswers : PageModel
{
    public void OnGet()
    {
    }
}
