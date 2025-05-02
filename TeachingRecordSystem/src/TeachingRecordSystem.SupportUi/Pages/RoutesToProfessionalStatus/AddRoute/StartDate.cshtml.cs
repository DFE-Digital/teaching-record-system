using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class StartDateModel : AddRouteCommonPageModel
{
    public StartDateModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache) : base(linkGenerator, referenceDataCache)
    {
    }

    [BindProperty]
    [DateInput(ErrorMessagePrefix = "Start date")]
    [Required(ErrorMessage = "Enter a start date")]
    [Display(Name = "Enter the route start date")]
    public DateOnly? TrainingStartDate { get; set; }

    public string BackLink => FromCheckAnswers ?
        LinkGenerator.RouteAddCheckYourAnswers(PersonId, JourneyInstance!.InstanceId) :
        LinkGenerator.RouteAddPage(PreviousPage(AddRoutePage.StartDate) ?? AddRoutePage.Status, PersonId, JourneyInstance!.InstanceId);

    public void OnGet()
    {
        TrainingStartDate = JourneyInstance!.State.TrainingStartDate;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }
        await JourneyInstance!.UpdateStateAsync(state => state.TrainingStartDate = TrainingStartDate);
        if (TrainingStartDate.HasValue && JourneyInstance!.State.TrainingEndDate is DateOnly endDate && TrainingStartDate >= endDate)
        {
            return Redirect(LinkGenerator.RouteAddEndDate(PersonId, JourneyInstance.InstanceId));
        }
        return Redirect(FromCheckAnswers ?
            LinkGenerator.RouteAddCheckYourAnswers(PersonId, JourneyInstance.InstanceId) :
            LinkGenerator.RouteAddPage(NextPage(AddRoutePage.StartDate) ?? AddRoutePage.CheckYourAnswers, PersonId, JourneyInstance!.InstanceId));
    }
}

