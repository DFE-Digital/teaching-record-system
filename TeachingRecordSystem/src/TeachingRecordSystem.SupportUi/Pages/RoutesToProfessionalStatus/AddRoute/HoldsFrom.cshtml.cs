using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class HoldsFromModel(IClock clock, TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache) : AddRouteCommonPageModel(linkGenerator, referenceDataCache)
{
    [BindProperty]
    [DateInput(ErrorMessagePrefix = "Professional status date")]
    [Required(ErrorMessage = "Enter the professional status date")]
    [Display(Name = "Enter a professional status date")]
    public DateOnly? HoldsFrom { get; set; }

    public string BackLink => FromCheckAnswers ?
        LinkGenerator.RouteAddCheckYourAnswers(PersonId, JourneyInstance!.InstanceId) :
        LinkGenerator.RouteAddPage(PreviousPage(AddRoutePage.AwardDate) ?? AddRoutePage.Status, PersonId, JourneyInstance!.InstanceId);

    public void OnGet()
    {
        HoldsFrom = JourneyInstance!.State.HoldsFrom;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (HoldsFrom > clock.Today)
        {
            ModelState.AddModelError(nameof(HoldsFrom), "Professional status date must not be in the future");
        }
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }
        await JourneyInstance!.UpdateStateAsync(state => state.HoldsFrom = HoldsFrom);

        return Redirect(FromCheckAnswers ?
            LinkGenerator.RouteAddCheckYourAnswers(PersonId, JourneyInstance.InstanceId) :
            LinkGenerator.RouteAddPage(NextPage(AddRoutePage.AwardDate) ?? AddRoutePage.CheckYourAnswers, PersonId, JourneyInstance!.InstanceId));
    }
}
