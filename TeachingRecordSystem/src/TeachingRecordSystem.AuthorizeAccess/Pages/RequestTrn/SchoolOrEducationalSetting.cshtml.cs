using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.WebCommon.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages.RequestTrn;

[Journey(RequestTrnJourneyState.JourneyName), RequireJourneyInstance]
public class SchoolOrEducationalSettingModel(AuthorizeAccessLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<RequestTrnJourneyState>? JourneyInstance { get; set; }

    [BindProperty]
    [Display(Name = "Are you currently working in a school or other educational setting?")]
    [Required(ErrorMessage = "Select yes if you’re currently working in a school or other educational setting")]
    public bool? IsWorkingInSchoolOrEducationalSetting { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state => state.WorkingInSchoolOrEducationalSetting = IsWorkingInSchoolOrEducationalSetting);

        return IsWorkingInSchoolOrEducationalSetting == false ?
            Redirect(linkGenerator.RequestTrnPersonalEmail(JourneyInstance!.InstanceId)) :
            Redirect(linkGenerator.RequestTrnWorkEmail(JourneyInstance.InstanceId));
    }
}
