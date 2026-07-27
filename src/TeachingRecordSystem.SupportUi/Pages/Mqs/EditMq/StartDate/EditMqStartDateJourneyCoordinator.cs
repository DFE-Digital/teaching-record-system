namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.StartDate;

[JourneyCoordinator(JourneyNames.EditMqStartDate, routeValueKeys: ["qualificationId"])]
public class EditMqStartDateJourneyCoordinator : JourneyCoordinator<EditMqStartDateState>
{
    public override EditMqStartDateState GetStartingState()
    {
        var qualificationInfo = HttpContext.GetCurrentMandatoryQualificationFeature();

        return new EditMqStartDateState
        {
            CurrentStartDate = qualificationInfo.MandatoryQualification.StartDate,
            StartDate = qualificationInfo.MandatoryQualification.StartDate
        };
    }
}
