using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class AwardDateModel : AddRouteCommonPageModel
{
    public AwardDateModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache) : base(linkGenerator, referenceDataCache)
    {
    }

    [BindProperty]
    [DateInput(ErrorMessagePrefix = "Award date")]
    [Required(ErrorMessage = "Enter the professional status award date")]
    [Display(Name = "Enter an award date")]
    public DateOnly? AwardedDate { get; set; }

    public string BackLink => FromCheckAnswers ?
        _linkGenerator.RouteAddCheckYourAnswers(PersonId, JourneyInstance!.InstanceId) :
        _linkGenerator.RouteAddPage(PreviousPage(AddRoutePage.AwardDate) ?? AddRoutePage.Status, PersonId, JourneyInstance!.InstanceId);

    public void OnGet()
    {
        AwardedDate = JourneyInstance!.State.AwardedDate;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }
        await JourneyInstance!.UpdateStateAsync(state => state.AwardedDate = AwardedDate);

        return Redirect(FromCheckAnswers ?
            _linkGenerator.RouteAddCheckYourAnswers(PersonId, JourneyInstance.InstanceId) :
            _linkGenerator.RouteAddPage(NextPage(AddRoutePage.AwardDate) ?? AddRoutePage.CheckYourAnswers, PersonId, JourneyInstance!.InstanceId));
    }
}
