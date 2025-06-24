using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class StartAndEndDateModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache) : AddRouteCommonPageModel(linkGenerator, referenceDataCache)
{
    [BindProperty]
    [DateInput(ErrorMessagePrefix = "Start date")]
    [Required(ErrorMessage = "Enter a start date")]
    [Display(Name = "Route start date")]
    public DateOnly? TrainingStartDate { get; set; }

    [BindProperty]
    [DateInput(ErrorMessagePrefix = "End date")]
    [Required(ErrorMessage = "Enter an end date")]
    [Display(Name = "Route end date")]
    public DateOnly? TrainingEndDate { get; set; }

    protected override RoutePage CurrentPage => RoutePage.StartAndEndDate;

    public string BackLink => PreviousPageUrl;

    public void OnGet()
    {
        TrainingStartDate = JourneyInstance!.State.NewRouteToProfessionalStatusId != null ? JourneyInstance!.State.NewTrainingStartDate : JourneyInstance!.State.TrainingStartDate;
        TrainingEndDate = JourneyInstance!.State.NewRouteToProfessionalStatusId != null ? JourneyInstance!.State.NewTrainingEndDate : JourneyInstance!.State.TrainingEndDate;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (TrainingStartDate >= TrainingEndDate)
        {
            ModelState.AddModelError(nameof(TrainingEndDate), "End date must be after start date");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(s =>
        {
            if (JourneyInstance!.State.NewRouteToProfessionalStatusId == null)
            {
                s.Begin();
            }

            s.NewTrainingStartDate = TrainingStartDate;
            s.NewTrainingEndDate = TrainingEndDate;
        });

        return await ContinueAsync();
    }
}
