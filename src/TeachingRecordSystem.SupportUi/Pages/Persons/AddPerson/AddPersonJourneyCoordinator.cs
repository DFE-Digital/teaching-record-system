namespace TeachingRecordSystem.SupportUi.Pages.Persons.AddPerson;

[JourneyCoordinator(JourneyNames.AddPerson, routeValueKeys: [])]
public class AddPersonJourneyCoordinator : JourneyCoordinator<AddPersonState>
{
    public override AddPersonState GetStartingState() => new();
}
