namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

public class AgeRangeSpecialismModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache) : AddRouteCommonPageModel(linkGenerator, referenceDataCache)
{
    public string BackLink => FromCheckAnswers ?
         _linkGenerator.RouteAddCheckYourAnswers(PersonId, JourneyInstance!.InstanceId) :
         _linkGenerator.RouteAddPage(PreviousPage(AddRoutePage.AgeRangeSpecialism) ?? AddRoutePage.Status, PersonId, JourneyInstance!.InstanceId);

    public void OnPost()
    {
        Redirect(FromCheckAnswers ?
            _linkGenerator.RouteAddCheckYourAnswers(PersonId, JourneyInstance!.InstanceId) :
            _linkGenerator.RouteAddPage(NextPage(AddRoutePage.AgeRangeSpecialism) ?? AddRoutePage.CheckYourAnswers, PersonId, JourneyInstance!.InstanceId));
    }

}
