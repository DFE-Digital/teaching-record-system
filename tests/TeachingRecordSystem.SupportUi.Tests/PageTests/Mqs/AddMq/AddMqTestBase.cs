using GovUk.Questions.AspNetCore.State;
using TeachingRecordSystem.SupportUi.Pages.Mqs.AddMq;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.AddMq;

public abstract class AddMqTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    protected Task<AddMqJourneyCoordinator> CreateJourneyInstanceForStateAsync(Guid personId, AddMqState state) =>
        // Seed the whole journey path so that any page under test is reachable (the real journey builds
        // this path up as the user advances through the steps).
        JourneyHelper.CreateInstanceAsync<AddMqJourneyCoordinator>(
            JourneyNames.AddMq,
            new RouteValueDictionary { ["personId"] = personId },
            _ => Task.FromResult<object>(state),
            pathUrls:
            [
                $"/mqs/add/provider?personId={personId}",
                $"/mqs/add/specialism?personId={personId}",
                $"/mqs/add/start-date?personId={personId}",
                $"/mqs/add/status?personId={personId}",
                $"/mqs/add/reason?personId={personId}",
                $"/mqs/add/check-answers?personId={personId}",
            ]);

    protected AddMqState? GetJourneyInstanceState(AddMqJourneyCoordinator coordinator)
    {
        var stateStorage = HostFixture.Services.GetRequiredService<IJourneyStateStorage>();
        return (AddMqState?)stateStorage.GetState(coordinator.InstanceId, coordinator.Journey)?.State;
    }
}
