using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.SupportUi.ValidationAttributes;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class AgeRangeSpecialismModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache)
    : AddRoutePostStatusPageModel(AddRoutePage.AgeRangeSpecialism, linkGenerator, referenceDataCache)
{
    [BindProperty]
    [AgeRangeRequiredValidation]
    [Display(Name = "Add age range specialism")]
    public AgeRange TrainingAgeSpecialism { get; set; } = new();

    public void OnGet()
    {
        TrainingAgeSpecialism = new AgeRange
        {
            AgeRangeFrom = JourneyInstance!.State.TrainingAgeSpecialismRangeFrom,
            AgeRangeTo = JourneyInstance!.State.TrainingAgeSpecialismRangeTo,
            AgeRangeType = JourneyInstance!.State.TrainingAgeSpecialismType.ToAgeSpecializationOption()
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.TrainingAgeSpecialismRangeFrom = TrainingAgeSpecialism!.AgeRangeFrom;
            state.TrainingAgeSpecialismRangeTo = TrainingAgeSpecialism!.AgeRangeTo;
            state.TrainingAgeSpecialismType = TrainingAgeSpecialism!.AgeRangeType.ToTrainingAgeSpecialismType();
        });

        return await ContinueAsync();
    }
}
