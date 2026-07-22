namespace TeachingRecordSystem.SupportUi.Pages.OneLogins.OneLoginDetail.DisconnectPerson;

// Both route values scope the journey: a disconnect is about a specific One Login *and* person pair,
// and scoping by both is what lets the library carry them through every generated URL.
[JourneyCoordinator(JourneyNames.DisconnectPerson, routeValueKeys: ["oneLoginUserSubject", "personId"])]
public class DisconnectPersonJourneyCoordinator : JourneyCoordinator<DisconnectPersonState>
{
    public override DisconnectPersonState GetStartingState() => new();
}
