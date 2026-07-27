using GovUk.Questions.AspNetCore.State;
using TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Specialism;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.EditMq.Specialism;

public abstract class EditMqSpecialismTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    protected Task<EditMqSpecialismJourneyCoordinator> CreateJourneyInstanceAsync(Guid qualificationId, EditMqSpecialismState? state = null) =>
        // Seed the whole journey path so that any page under test is reachable (the real journey builds
        // this path up as the user advances through the steps).
        JourneyHelper.CreateInstanceAsync<EditMqSpecialismJourneyCoordinator>(
            JourneyNames.EditMqSpecialism,
            new RouteValueDictionary { ["qualificationId"] = qualificationId },
            _ => Task.FromResult<object>(state ?? new EditMqSpecialismState()),
            pathUrls:
            [
                $"/mqs/{qualificationId}/specialism",
                $"/mqs/{qualificationId}/specialism/reason",
                $"/mqs/{qualificationId}/specialism/check-answers",
            ]);

    protected EditMqSpecialismState? GetJourneyInstanceState(EditMqSpecialismJourneyCoordinator coordinator)
    {
        var stateStorage = HostFixture.Services.GetRequiredService<IJourneyStateStorage>();
        return (EditMqSpecialismState?)stateStorage.GetState(coordinator.InstanceId, coordinator.Journey)?.State;
    }
}
