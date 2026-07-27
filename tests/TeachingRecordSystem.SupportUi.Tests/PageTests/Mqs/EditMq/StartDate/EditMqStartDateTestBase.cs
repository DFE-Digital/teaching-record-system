using GovUk.Questions.AspNetCore.State;
using TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.StartDate;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.EditMq.StartDate;

public abstract class EditMqStartDateTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    protected Task<EditMqStartDateJourneyCoordinator> CreateJourneyInstanceAsync(Guid qualificationId, EditMqStartDateState? state = null) =>
        // Seed the whole journey path so that any page under test is reachable (the real journey builds
        // this path up as the user advances through the steps).
        JourneyHelper.CreateInstanceAsync<EditMqStartDateJourneyCoordinator>(
            JourneyNames.EditMqStartDate,
            new RouteValueDictionary { ["qualificationId"] = qualificationId },
            _ => Task.FromResult<object>(state ?? new EditMqStartDateState()),
            pathUrls:
            [
                $"/mqs/{qualificationId}/start-date",
                $"/mqs/{qualificationId}/start-date/reason",
                $"/mqs/{qualificationId}/start-date/check-answers",
            ]);

    protected EditMqStartDateState? GetJourneyInstanceState(EditMqStartDateJourneyCoordinator coordinator)
    {
        var stateStorage = HostFixture.Services.GetRequiredService<IJourneyStateStorage>();
        return (EditMqStartDateState?)stateStorage.GetState(coordinator.InstanceId, coordinator.Journey)?.State;
    }
}
