using GovUk.Questions.AspNetCore.State;
using TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Status;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.EditMq.Status;

public abstract class EditMqStatusTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    protected Task<EditMqStatusJourneyCoordinator> CreateJourneyInstanceAsync(Guid qualificationId, EditMqStatusState? state = null) =>
        // Seed the whole journey path so that any page under test is reachable (the real journey builds
        // this path up as the user advances through the steps).
        JourneyHelper.CreateInstanceAsync<EditMqStatusJourneyCoordinator>(
            JourneyNames.EditMqStatus,
            new RouteValueDictionary { ["qualificationId"] = qualificationId },
            _ => Task.FromResult<object>(state ?? new EditMqStatusState()),
            pathUrls:
            [
                $"/mqs/{qualificationId}/status",
                $"/mqs/{qualificationId}/status/reason",
                $"/mqs/{qualificationId}/status/check-answers",
            ]);

    protected EditMqStatusState? GetJourneyInstanceState(EditMqStatusJourneyCoordinator coordinator)
    {
        var stateStorage = HostFixture.Services.GetRequiredService<IJourneyStateStorage>();
        return (EditMqStatusState?)stateStorage.GetState(coordinator.InstanceId, coordinator.Journey)?.State;
    }
}
