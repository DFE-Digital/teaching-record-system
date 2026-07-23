namespace TeachingRecordSystem.SupportUi.Pages.Mqs.DeleteMq;

[JourneyCoordinator(JourneyNames.DeleteMq, routeValueKeys: ["qualificationId"])]
public class DeleteMqJourneyCoordinator : JourneyCoordinator<DeleteMqState>
{
    public override DeleteMqState GetStartingState() => new();
}
