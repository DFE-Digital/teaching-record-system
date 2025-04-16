namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

public class TrainingProviderModel : AddRouteCommonPageModel
{
    public string BackLink => FromCheckAnswers ?
        _linkGenerator.RouteAddCheckAnswers(PersonId, JourneyInstance!.InstanceId) :
        _linkGenerator.RouteAddPage(PreviousPage(AddRoutePage.TrainingProvider) ?? AddRoutePage.Status, PersonId, JourneyInstance!.InstanceId);

    public TrainingProviderModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache) : base(linkGenerator, referenceDataCache)
    {

    }
    public void OnGet()
    {
    }
}
