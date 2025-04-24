using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class AgeRangeSpecialismModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache) : AddRouteCommonPageModel(linkGenerator, referenceDataCache)
{
    public string BackLink => FromCheckAnswers ?
         _linkGenerator.RouteAddCheckYourAnswers(PersonId, JourneyInstance!.InstanceId) :
         _linkGenerator.RouteAddPage(PreviousPage(AddRoutePage.AgeRangeSpecialism) ?? AddRoutePage.Status, PersonId, JourneyInstance!.InstanceId);

    [BindProperty]
    [Display(Name = "Edit age range specialism")]
    public AgeRange TrainingAgeSpecialism { get; set; } = new();

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
            _linkGenerator.RouteAddCheckYourAnswers(PersonId, JourneyInstance!.InstanceId) :
            _linkGenerator.RouteAddPage(NextPage(AddRoutePage.AgeRangeSpecialism) ?? AddRoutePage.CheckYourAnswers, PersonId, JourneyInstance!.InstanceId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonName = personInfo.Name;
        PersonId = personInfo.PersonId;
    }
}
