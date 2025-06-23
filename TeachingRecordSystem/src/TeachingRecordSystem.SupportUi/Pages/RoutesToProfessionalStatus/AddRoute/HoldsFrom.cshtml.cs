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

    protected override RoutePage CurrentPage => RoutePage.HoldsFrom;

    public string BackLink => PreviousPage;

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

        return Redirect(NextPage);
    }
}
