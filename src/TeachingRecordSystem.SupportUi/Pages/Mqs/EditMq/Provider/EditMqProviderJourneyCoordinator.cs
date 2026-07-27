namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Provider;

[JourneyCoordinator(JourneyNames.EditMqProvider, routeValueKeys: ["qualificationId"])]
public class EditMqProviderJourneyCoordinator : JourneyCoordinator<EditMqProviderState>
{
    public override EditMqProviderState GetStartingState()
    {
        var qualificationInfo = HttpContext.GetCurrentMandatoryQualificationFeature();

        return new EditMqProviderState
        {
            CurrentProviderId = qualificationInfo.MandatoryQualification.ProviderId,
            ProviderId = qualificationInfo.MandatoryQualification.ProviderId
        };
    }
}
