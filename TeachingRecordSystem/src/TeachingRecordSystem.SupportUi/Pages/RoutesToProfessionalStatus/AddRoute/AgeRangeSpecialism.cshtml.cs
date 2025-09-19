using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class AgeRangeSpecialismModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache)
    : AddRoutePostStatusPageModel(AddRoutePage.AgeRangeSpecialism, linkGenerator, referenceDataCache)
{
    [BindProperty]
    public AgeRange TrainingAgeSpecialism { get; set; } = new();

    public bool AgeRangeSpecialismRequired => QuestionDriverHelper.FieldRequired(RouteType.TrainingAgeSpecialismTypeRequired, Status.GetAgeSpecialismRequirement())
        == FieldRequirement.Mandatory;

    public string PageHeading => "Select an age range specialism"
       + (AgeRangeSpecialismRequired ? "" : " (optional)");

    public void OnGet()
    {
        TrainingAgeSpecialism = new AgeRange
        {
            AgeRangeFrom = JourneyInstance!.State.TrainingAgeSpecialismRangeFrom,
            AgeRangeTo = JourneyInstance!.State.TrainingAgeSpecialismRangeTo,
            AgeRangeType = JourneyInstance!.State.TrainingAgeSpecialismType
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (AgeRangeSpecialismRequired && TrainingAgeSpecialism.AgeRangeType is null)
        {
            ModelState.AddModelError($"{nameof(TrainingAgeSpecialism)}.{nameof(TrainingAgeSpecialism.AgeRangeType)}", "Enter an age range specialism");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.TrainingAgeSpecialismRangeFrom = TrainingAgeSpecialism!.AgeRangeFrom;
            state.TrainingAgeSpecialismRangeTo = TrainingAgeSpecialism!.AgeRangeTo;
            state.TrainingAgeSpecialismType = TrainingAgeSpecialism!.AgeRangeType;
        });

        return await ContinueAsync();
    }
}
