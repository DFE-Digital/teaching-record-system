using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class StartAndEndDateModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache)
    : AddRoutePostStatusPageModel(AddRoutePage.StartAndEndDate, linkGenerator, referenceDataCache)
{
    [BindProperty]
    [DateInput(ErrorMessagePrefix = "Start date")]
    [Display(Name = "Route start date")]
    public DateOnly? TrainingStartDate { get; set; }

    [BindProperty]
    [DateInput(ErrorMessagePrefix = "End date")]
    [Display(Name = "Route end date")]
    public DateOnly? TrainingEndDate { get; set; }

    public bool StartAndEndDatesRequired => QuestionDriverHelper.FieldRequired(RouteType.TrainingEndDateRequired, Status.GetEndDateRequirement())
        == FieldRequirement.Mandatory;

    public string PageHeading => "Enter the route start and end dates"
       + (StartAndEndDatesRequired ? "" : " (optional)");

    public void OnGet()
    {
        TrainingStartDate = JourneyInstance!.State.TrainingStartDate;
        TrainingEndDate = JourneyInstance!.State.TrainingEndDate;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (StartAndEndDatesRequired)
        {
            if (TrainingStartDate is null)
            {
                ModelState.AddModelError(nameof(TrainingStartDate), "Enter a start date");
            }

            if (TrainingEndDate is null)
            {
                ModelState.AddModelError(nameof(TrainingEndDate), "Enter an end date");
            }
        }

        if (TrainingStartDate >= TrainingEndDate)
        {
            ModelState.AddModelError(nameof(TrainingEndDate), "End date must be after start date");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.TrainingStartDate = TrainingStartDate;
            state.TrainingEndDate = TrainingEndDate;
        });

        return await ContinueAsync();
    }
}
