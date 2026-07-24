namespace TeachingRecordSystem.SupportUi.Pages.Mqs.AddMq;

[JourneyCoordinator(JourneyNames.AddMq, routeValueKeys: ["personId"])]
public class AddMqJourneyCoordinator : JourneyCoordinator<AddMqState>
{
    public override AddMqState GetStartingState() => new();
}
