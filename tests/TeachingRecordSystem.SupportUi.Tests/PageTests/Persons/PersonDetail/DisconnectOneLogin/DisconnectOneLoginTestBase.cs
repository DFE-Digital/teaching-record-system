using GovUk.Questions.AspNetCore.State;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.DisconnectOneLogin;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.DisconnectOneLogin;

public abstract class DisconnectOneLoginTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    protected Task<DisconnectOneLoginJourneyCoordinator> CreateJourneyInstanceAsync(
        Guid personId,
        string oneLoginSubject,
        DisconnectOneLoginState? state = null)
    {
        var basePath = $"/persons/{personId}/disconnect-one-login/{oneLoginSubject}";

        // Seed the whole journey path so that any page under test is reachable (the real journey builds
        // this path up as the user advances through the steps).
        return JourneyHelper.CreateInstanceAsync<DisconnectOneLoginJourneyCoordinator>(
            JourneyNames.DisconnectOneLogin,
            new RouteValueDictionary
            {
                ["personId"] = personId,
                ["oneLoginSubject"] = oneLoginSubject
            },
            _ => Task.FromResult<object>(state ?? new DisconnectOneLoginState()),
            pathUrls: [basePath, $"{basePath}/verified", $"{basePath}/check-answers"]);
    }

    protected DisconnectOneLoginState? GetJourneyInstanceState(DisconnectOneLoginJourneyCoordinator coordinator)
    {
        var stateStorage = HostFixture.Services.GetRequiredService<IJourneyStateStorage>();
        return (DisconnectOneLoginState?)stateStorage.GetState(coordinator.InstanceId, coordinator.Journey)?.State;
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
