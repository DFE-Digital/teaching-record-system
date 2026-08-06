using GovUk.Questions.AspNetCore.State;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.DeleteRoute;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus.DeleteRoute;

public abstract class DeleteRouteTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    protected Task<DeleteRouteJourneyCoordinator> CreateJourneyInstanceAsync(Guid qualificationId, DeleteRouteState? state = null) =>
        // Seed the whole journey path so that any page under test is reachable (the real journey builds
        // this path up as the user advances through the steps).
        JourneyHelper.CreateInstanceAsync<DeleteRouteJourneyCoordinator>(
            JourneyNames.DeleteRouteToProfessionalStatus,
            new RouteValueDictionary { ["qualificationId"] = qualificationId },
            _ => Task.FromResult<object>(state ?? new DeleteRouteState()),
            pathUrls:
            [
                $"/routes/{qualificationId}/delete/reason",
                $"/routes/{qualificationId}/delete/check-answers"
            ],
            coordinatorFactory: CreateJourneyCoordinator<DeleteRouteJourneyCoordinator>);

    protected DeleteRouteState? GetJourneyInstanceState(DeleteRouteJourneyCoordinator coordinator)
    {
        var stateStorage = HostFixture.Services.GetRequiredService<IJourneyStateStorage>();
        return (DeleteRouteState?)stateStorage.GetState(coordinator.InstanceId, coordinator.Journey)?.State;
    }
}
