using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class AgeRangeSpecialismModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache)
    : AddRouteCommonPageModel(linkGenerator, referenceDataCache)
{
    protected override RoutePage CurrentPage => RoutePage.AgeRangeSpecialism;

    public string BackLink => PreviousPageUrl;

    [BindProperty]
    [Display(Name = "Add age range specialism")]
    public AgeRange TrainingAgeSpecialism { get; set; } = new();

    public void OnGet()
    {
        TrainingAgeSpecialism = new AgeRange
        {
            AgeRangeFrom = JourneyInstance!.State.NewRouteToProfessionalStatusId != null ? JourneyInstance!.State.NewTrainingAgeSpecialismRangeFrom : JourneyInstance!.State.TrainingAgeSpecialismRangeFrom,
            AgeRangeTo = JourneyInstance!.State.NewRouteToProfessionalStatusId != null ? JourneyInstance!.State.NewTrainingAgeSpecialismRangeTo : JourneyInstance!.State.TrainingAgeSpecialismRangeTo,
            AgeRangeType = JourneyInstance!.State.NewRouteToProfessionalStatusId != null ? JourneyInstance!.State.NewTrainingAgeSpecialismType : JourneyInstance!.State.TrainingAgeSpecialismType
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
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

            s.NewTrainingAgeSpecialismRangeFrom = TrainingAgeSpecialism!.AgeRangeFrom;
            s.NewTrainingAgeSpecialismRangeTo = TrainingAgeSpecialism!.AgeRangeTo;
            s.NewTrainingAgeSpecialismType = TrainingAgeSpecialism!.AgeRangeType;
        });

        return await ContinueAsync();
    }
}
