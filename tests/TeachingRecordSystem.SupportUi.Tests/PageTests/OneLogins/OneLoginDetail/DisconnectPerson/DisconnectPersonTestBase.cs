using GovUk.Questions.AspNetCore.State;
using TeachingRecordSystem.SupportUi.Pages.OneLogins.OneLoginDetail.DisconnectPerson;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.OneLogins.OneLoginDetail.DisconnectPerson;

public abstract class DisconnectPersonTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    protected Task<DisconnectPersonJourneyCoordinator> CreateJourneyInstanceAsync(
        string oneLoginUserSubject,
        Guid personId,
        DisconnectPersonState? state = null)
    {
        var basePath = $"/one-logins/{oneLoginUserSubject}/disconnect-person/{personId}";

        // Seed the whole journey path so that any page under test is reachable (the real journey builds
        // this path up as the user advances through the steps).
        return JourneyHelper.CreateInstanceAsync<DisconnectPersonJourneyCoordinator>(
            JourneyNames.DisconnectPerson,
            new RouteValueDictionary
            {
                ["oneLoginUserSubject"] = oneLoginUserSubject,
                ["personId"] = personId
            },
            _ => Task.FromResult<object>(state ?? new DisconnectPersonState()),
            pathUrls: [basePath, $"{basePath}/verified", $"{basePath}/check-answers"]);
    }

    protected DisconnectPersonState? GetJourneyInstanceState(DisconnectPersonJourneyCoordinator coordinator)
    {
        var stateStorage = HostFixture.Services.GetRequiredService<IJourneyStateStorage>();
        return (DisconnectPersonState?)stateStorage.GetState(coordinator.InstanceId, coordinator.Journey)?.State;
    }

    /// <summary>
    /// Submits the page's Cancel button exactly as the rendered page defines it, rather than a URL of
    /// our own choosing.
    /// </summary>
    protected async Task<HttpResponseMessage> PostCancelAsync(string pageUrl)
    {
        var doc = await AssertEx.HtmlResponseAsync(await HttpClient.GetAsync(pageUrl));
        var cancelButton = doc.GetElementByTestId("cancel-button")!;
        Assert.Null(cancelButton.GetAttribute("formaction"));

        return await HttpClient.PostAsync(
            pageUrl,
            new FormUrlEncodedContentBuilder
            {
                { cancelButton.GetAttribute("name")!, cancelButton.GetAttribute("value")! }
            });
    }
}
