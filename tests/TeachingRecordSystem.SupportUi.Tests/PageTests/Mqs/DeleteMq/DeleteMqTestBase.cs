using GovUk.Questions.AspNetCore.State;
using TeachingRecordSystem.SupportUi.Pages.Mqs.DeleteMq;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.DeleteMq;

public abstract class DeleteMqTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    protected Task<DeleteMqJourneyCoordinator> CreateJourneyInstanceAsync(Guid qualificationId, DeleteMqState? state = null) =>
        // Seed the whole journey path so that any page under test is reachable (the real journey builds
        // this path up as the user advances through the steps).
        JourneyHelper.CreateInstanceAsync<DeleteMqJourneyCoordinator>(
            JourneyNames.DeleteMq,
            new RouteValueDictionary { ["qualificationId"] = qualificationId },
            _ => Task.FromResult<object>(state ?? new DeleteMqState()),
            pathUrls:
            [
                $"/mqs/{qualificationId}/delete",
                $"/mqs/{qualificationId}/delete/check-answers",
            ]);

    protected DeleteMqState? GetJourneyInstanceState(DeleteMqJourneyCoordinator coordinator)
    {
        var stateStorage = HostFixture.Services.GetRequiredService<IJourneyStateStorage>();
        return (DeleteMqState?)stateStorage.GetState(coordinator.InstanceId, coordinator.Journey)?.State;
    }
}
