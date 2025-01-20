using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.WebCommon.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages.RequestTrn;

[Journey(RequestTrnJourneyState.JourneyName), RequireJourneyInstance]
public class NpqApplicationModel(AuthorizeAccessLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<RequestTrnJourneyState>? JourneyInstance { get; set; }

    [FromQuery]
    public bool? FromCheckAnswers { get; set; }

    [BindProperty]
    [Display(Name = "What is your NPQ application ID?", Description = "We’ll use this to check your registration details.")]
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
            return Redirect(linkGenerator.RequestTrnCheckAnswers(JourneyInstance!.InstanceId));
        }
        else
        {
            return Redirect(linkGenerator.RequestTrnWorkingInSchoolOrEducationalSetting(JourneyInstance.InstanceId));
        }

    }
}
