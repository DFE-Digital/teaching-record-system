using GovUk.Questions.AspNetCore.State;
using TeachingRecordSystem.SupportUi.Pages.Persons.AddPerson;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.AddPerson;

public abstract class AddPersonTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    protected Task<AddPersonJourneyCoordinator> CreateJourneyInstanceAsync(AddPersonState? state = null) =>
        // Seed the whole journey path so that any page under test is reachable (the real journey builds
        // this path up as the user advances through the steps).
        JourneyHelper.CreateInstanceAsync<AddPersonJourneyCoordinator>(
            JourneyNames.AddPerson,
            new RouteValueDictionary(),
            _ => Task.FromResult<object>(state ?? new AddPersonState()),
            pathUrls:
            [
                "/persons/add/personal-details",
                "/persons/add/reason",
                "/persons/add/check-answers",
            ]);

    protected AddPersonState? GetJourneyInstanceState(AddPersonJourneyCoordinator coordinator)
    {
        var stateStorage = HostFixture.Services.GetRequiredService<IJourneyStateStorage>();
        return (AddPersonState?)stateStorage.GetState(coordinator.InstanceId, coordinator.Journey)?.State;
    }
}
