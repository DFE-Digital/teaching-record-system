using GovUk.Questions.AspNetCore.State;
using TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Provider;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.EditMq.Provider;

public abstract class EditMqProviderTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    protected Task<EditMqProviderJourneyCoordinator> CreateJourneyInstanceAsync(Guid qualificationId, EditMqProviderState? state = null) =>
        // Seed the whole journey path so that any page under test is reachable (the real journey builds
        // this path up as the user advances through the steps).
        JourneyHelper.CreateInstanceAsync<EditMqProviderJourneyCoordinator>(
            JourneyNames.EditMqProvider,
            new RouteValueDictionary { ["qualificationId"] = qualificationId },
            _ => Task.FromResult<object>(state ?? new EditMqProviderState()),
            pathUrls:
            [
                $"/mqs/{qualificationId}/provider",
                $"/mqs/{qualificationId}/provider/reason",
                $"/mqs/{qualificationId}/provider/check-answers",
            ]);

    protected EditMqProviderState? GetJourneyInstanceState(EditMqProviderJourneyCoordinator coordinator)
    {
        var stateStorage = HostFixture.Services.GetRequiredService<IJourneyStateStorage>();
        return (EditMqProviderState?)stateStorage.GetState(coordinator.InstanceId, coordinator.Journey)?.State;
    }
}
