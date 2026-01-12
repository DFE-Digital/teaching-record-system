using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.WebCommon.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages.RequestTrn;

[WebCommon.FormFlow.Journey(RequestTrnJourneyState.JourneyName), RequireJourneyInstance]
public class NpqApplicationModel(RequestTrnLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<RequestTrnJourneyState>? JourneyInstance { get; set; }

    [FromQuery]
    public bool? FromCheckAnswers { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Enter your NPQ application ID")]
    public string? NpqApplicationId { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state => state.NpqApplicationId = NpqApplicationId);

        if (FromCheckAnswers == true)
        {
            return Redirect(linkGenerator.CheckAnswers(JourneyInstance!.InstanceId));
        }
        else
        {
            return Redirect(linkGenerator.WorkingInSchoolOrEducationalSetting(JourneyInstance.InstanceId));
        }

    }
}
