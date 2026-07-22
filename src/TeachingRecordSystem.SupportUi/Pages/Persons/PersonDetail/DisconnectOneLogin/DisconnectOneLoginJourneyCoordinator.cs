namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.DisconnectOneLogin;

// Both route values scope the journey: a disconnect is about a specific person *and* One Login pair,
// and scoping by both is what lets the library carry them through every generated URL.
[JourneyCoordinator(JourneyNames.DisconnectOneLogin, routeValueKeys: ["personId", "oneLoginSubject"])]
public class DisconnectOneLoginJourneyCoordinator : JourneyCoordinator<DisconnectOneLoginState>
{
    public override DisconnectOneLoginState GetStartingState() => new();
}
