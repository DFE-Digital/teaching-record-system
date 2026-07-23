using GovUk.Questions.AspNetCore.State;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.ConnectOneLogin;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.ConnectOneLogin;

public abstract class ConnectOneLoginTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    protected Task<ConnectOneLoginJourneyCoordinator> CreateJourneyInstanceAsync(
        Guid personId,
        ConnectOneLoginState state)
    {
        var basePath = $"/persons/{personId}/connect-one-login";

        // Seed the whole journey path so that any page under test is reachable (the real journey builds
        // this path up as the user advances through the steps).
        return JourneyHelper.CreateInstanceAsync<ConnectOneLoginJourneyCoordinator>(
            JourneyNames.ConnectOneLogin,
            new RouteValueDictionary { ["personId"] = personId },
            _ => Task.FromResult<object>(state),
            pathUrls:
            [
                basePath,
                $"{basePath}/match",
                $"{basePath}/reason",
                $"{basePath}/check-answers"
            ]);
    }

    protected ConnectOneLoginState? GetJourneyInstanceState(ConnectOneLoginJourneyCoordinator coordinator)
    {
        var stateStorage = HostFixture.Services.GetRequiredService<IJourneyStateStorage>();
        return (ConnectOneLoginState?)stateStorage.GetState(coordinator.InstanceId, coordinator.Journey)?.State;
    }
}
