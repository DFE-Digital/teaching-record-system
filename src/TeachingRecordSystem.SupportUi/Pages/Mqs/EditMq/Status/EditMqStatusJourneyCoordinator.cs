namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Status;

[JourneyCoordinator(JourneyNames.EditMqStatus, routeValueKeys: ["qualificationId"])]
public class EditMqStatusJourneyCoordinator : JourneyCoordinator<EditMqStatusState>
{
    public override EditMqStatusState GetStartingState()
    {
        var qualificationInfo = HttpContext.GetCurrentMandatoryQualificationFeature();

        return new EditMqStatusState
        {
            CurrentStatus = qualificationInfo.MandatoryQualification.Status,
            Status = qualificationInfo.MandatoryQualification.Status,
            CurrentEndDate = qualificationInfo.MandatoryQualification.EndDate,
            EndDate = qualificationInfo.MandatoryQualification.EndDate
        };
    }
}
