using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class AgeRangeSpecialismModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache)
    : AddRoutePostStatusPageModel(AddRoutePage.AgeRangeSpecialism, linkGenerator, referenceDataCache)
{
    public const string PageHeading = "Add age range specialism";

    [BindProperty]
    [Display(Name = PageHeading)]
    public AgeRange TrainingAgeSpecialism { get; set; } = new();

    public bool AgeRangeSpecialismRequired => QuestionDriverHelper.FieldRequired(Route.TrainingAgeSpecialismTypeRequired, Status.GetAgeSpecialismRequirement())
        == FieldRequirement.Mandatory;

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
