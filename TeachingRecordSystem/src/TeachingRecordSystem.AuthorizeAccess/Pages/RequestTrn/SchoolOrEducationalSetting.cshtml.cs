using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.WebCommon.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages.RequestTrn;

[WebCommon.FormFlow.Journey(RequestTrnJourneyState.JourneyName), RequireJourneyInstance]
public class SchoolOrEducationalSettingModel(RequestTrnLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<RequestTrnJourneyState>? JourneyInstance { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Select yes if you’re currently working in a school or other educational setting")]
    public bool? IsWorkingInSchoolOrEducationalSetting { get; set; }

    public void OnGet()
    {
        IsWorkingInSchoolOrEducationalSetting = JourneyInstance!.State.WorkingInSchoolOrEducationalSetting;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.WorkingInSchoolOrEducationalSetting = IsWorkingInSchoolOrEducationalSetting;
            state.WorkEmail = IsWorkingInSchoolOrEducationalSetting!.Value ? state.WorkEmail : null;
        });

        return IsWorkingInSchoolOrEducationalSetting == false ?
            Redirect(linkGenerator.PersonalEmail(JourneyInstance!.InstanceId)) :
            Redirect(linkGenerator.WorkEmail(JourneyInstance.InstanceId));
    }
}
