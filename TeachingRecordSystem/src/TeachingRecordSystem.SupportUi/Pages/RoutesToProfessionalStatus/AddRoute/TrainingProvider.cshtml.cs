using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class TrainingProviderModel : AddRouteCommonPageModel
{
    public string BackLink => FromCheckAnswers ?
        _linkGenerator.RouteAddCheckYourAnswers(PersonId, JourneyInstance!.InstanceId) :
        _linkGenerator.RouteAddPage(PreviousPage(AddRoutePage.TrainingProvider) ?? AddRoutePage.Status, PersonId, JourneyInstance!.InstanceId);

    public TrainingProviderModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache) : base(linkGenerator, referenceDataCache)
    {
    }

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        return Redirect(FromCheckAnswers ?
            _linkGenerator.RouteCheckYourAnswers(PersonId, JourneyInstance!.InstanceId) :
            _linkGenerator.RouteAddPage(NextPage(AddRoutePage.TrainingProvider) ?? AddRoutePage.CheckYourAnswers, PersonId, JourneyInstance!.InstanceId));
    }

}
