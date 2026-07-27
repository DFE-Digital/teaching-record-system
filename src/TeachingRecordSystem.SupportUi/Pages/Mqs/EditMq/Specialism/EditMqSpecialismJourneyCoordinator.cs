namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Specialism;

[JourneyCoordinator(JourneyNames.EditMqSpecialism, routeValueKeys: ["qualificationId"])]
public class EditMqSpecialismJourneyCoordinator : JourneyCoordinator<EditMqSpecialismState>
{
    public override EditMqSpecialismState GetStartingState()
    {
        var qualificationInfo = HttpContext.GetCurrentMandatoryQualificationFeature();

        return new EditMqSpecialismState
        {
            CurrentSpecialism = qualificationInfo.MandatoryQualification.Specialism,
            Specialism = qualificationInfo.MandatoryQualification.Specialism
        };
    }
}
