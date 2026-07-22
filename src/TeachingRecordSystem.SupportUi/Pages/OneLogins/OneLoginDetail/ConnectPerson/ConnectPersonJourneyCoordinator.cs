namespace TeachingRecordSystem.SupportUi.Pages.OneLogins.OneLoginDetail.ConnectPerson;

[JourneyCoordinator(JourneyNames.ConnectPerson, routeValueKeys: ["oneLoginUserSubject"])]
public class ConnectPersonJourneyCoordinator : JourneyCoordinator<ConnectPersonState>
{
    public override ConnectPersonState GetStartingState() => new();
}
