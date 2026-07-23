namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.ConnectOneLogin;

[JourneyCoordinator(JourneyNames.ConnectOneLogin, routeValueKeys: ["personId"])]
public class ConnectOneLoginJourneyCoordinator : JourneyCoordinator<ConnectOneLoginState>
{
    public override ConnectOneLoginState GetStartingState() => new();
}
