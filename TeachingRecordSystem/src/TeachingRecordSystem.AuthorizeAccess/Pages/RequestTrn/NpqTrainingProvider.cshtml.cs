using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.WebCommon.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages.RequestTrn;

[Journey(RequestTrnJourneyState.JourneyName), RequireJourneyInstance]
public class NpqTrainingProviderModel(AuthorizeAccessLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<RequestTrnJourneyState>? JourneyInstance { get; set; }

    [FromQuery]
    public bool? FromCheckAnswers { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Enter your NPQ training provider")]
    [MaxLength(200, ErrorMessage = "NPQ training provider must be 200 characters or less")]
    public string? NpqTrainingProvider { get; set; }

    public void OnGet()
    {
        NpqTrainingProvider = JourneyInstance!.State.NpqTrainingProvider;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.NpqTrainingProvider = NpqTrainingProvider;
        });

        return FromCheckAnswers == true ?
            Redirect(linkGenerator.RequestTrnCheckAnswers(JourneyInstance!.InstanceId)) :
            Redirect(linkGenerator.RequestTrnWorkingInSchoolOrEducationalSetting(JourneyInstance!.InstanceId));
    }
}
