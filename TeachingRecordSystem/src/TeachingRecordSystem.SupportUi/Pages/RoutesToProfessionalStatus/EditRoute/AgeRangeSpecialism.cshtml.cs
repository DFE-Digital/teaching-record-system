using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus), RequireJourneyInstance]
public class AgeRangeSpecialismModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache)
    : EditRouteCommonPageModel(linkGenerator, referenceDataCache)
{
    [BindProperty]
    public AgeRange TrainingAgeSpecialism { get; set; } = new();

    public bool AgeRangeSpecialismRequired => QuestionDriverHelper.FieldRequired(RouteType.TrainingAgeSpecialismTypeRequired, Status.GetAgeSpecialismRequirement())
        == FieldRequirement.Mandatory;

    public string PageHeading => "Edit age range specialism"
       + (AgeRangeSpecialismRequired ? "" : " (optional)");

    public void OnGet()
    {
        TrainingAgeSpecialism.AgeRangeFrom = JourneyInstance!.State.TrainingAgeSpecialismRangeFrom;
        TrainingAgeSpecialism.AgeRangeTo = JourneyInstance!.State.TrainingAgeSpecialismRangeTo;
        TrainingAgeSpecialism.AgeRangeType = JourneyInstance!.State.TrainingAgeSpecialismType;
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

        await JourneyInstance!.UpdateStateAsync(s =>
            {
                s.TrainingAgeSpecialismRangeFrom = TrainingAgeSpecialism!.AgeRangeFrom;
                s.TrainingAgeSpecialismRangeTo = TrainingAgeSpecialism!.AgeRangeTo;
                s.TrainingAgeSpecialismType = TrainingAgeSpecialism!.AgeRangeType;
            });

        return Redirect(FromCheckAnswers ?
            LinkGenerator.RouteEditCheckYourAnswers(QualificationId, JourneyInstance!.InstanceId) :
            LinkGenerator.RouteEditDetail(QualificationId, JourneyInstance!.InstanceId));
    }
}
