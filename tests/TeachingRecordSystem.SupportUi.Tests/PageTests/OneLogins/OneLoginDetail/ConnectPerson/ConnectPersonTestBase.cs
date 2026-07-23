using GovUk.Questions.AspNetCore.State;
using TeachingRecordSystem.SupportUi.Pages.OneLogins.OneLoginDetail.ConnectPerson;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.OneLogins.OneLoginDetail.ConnectPerson;

public abstract class ConnectPersonTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    protected Task<ConnectPersonJourneyCoordinator> CreateJourneyInstanceAsync(
        string oneLoginUserSubject,
        ConnectPersonState state)
    {
        var basePath = $"/one-logins/{oneLoginUserSubject}/connect-person";

        // Seed the whole journey path so that any page under test is reachable (the real journey builds
        // this path up as the user advances through the steps).
        return JourneyHelper.CreateInstanceAsync<ConnectPersonJourneyCoordinator>(
            JourneyNames.ConnectPerson,
            new RouteValueDictionary { ["oneLoginUserSubject"] = oneLoginUserSubject },
            _ => Task.FromResult<object>(state),
            pathUrls:
            [
                basePath,
                $"{basePath}/match",
                $"{basePath}/reason",
                $"{basePath}/check-answers"
            ]);
    }

    protected ConnectPersonState? GetJourneyInstanceState(ConnectPersonJourneyCoordinator coordinator)
    {
        var stateStorage = HostFixture.Services.GetRequiredService<IJourneyStateStorage>();
        return (ConnectPersonState?)stateStorage.GetState(coordinator.InstanceId, coordinator.Journey)?.State;
    }
}
