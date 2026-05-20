using System.Diagnostics;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using static TeachingRecordSystem.AuthorizeAccess.SignInJourneyCoordinator.Vtrs;

namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests;

public class FoundTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public Task Get_ValidRequest_RendersExpectedContent() =>
        WithJourneyCoordinatorAsync(
            CreateSignInJourneyState,
            async coordinator =>
            {
                // Arrange
                var person = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber());
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(person);

                await SetupInstanceStateAsync(coordinator, oneLoginUser, person.NationalInsuranceNumber!);

                var request = new HttpRequestMessage(HttpMethod.Get, JourneyUrls.Found(coordinator.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                await AssertEx.HtmlResponseAsync(response);
            });

    [Fact]
    public Task Get_WithoutAppContent_RendersDefaultLinkText() =>
        WithJourneyCoordinatorAsync(
            CreateSignInJourneyState,
            async coordinator =>
            {
                // Arrange
                var person = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber());
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(person);

                await SetupInstanceStateAsync(coordinator, oneLoginUser, person.NationalInsuranceNumber!);

                var request = new HttpRequestMessage(HttpMethod.Get, JourneyUrls.Found(coordinator.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                var doc = await AssertEx.HtmlResponseAsync(response);
                var continueLink = doc.GetElementByTestId("continue-link");
                Assert.NotNull(continueLink);
                Assert.Contains("access your teaching record", continueLink.TextContent);
                Assert.Equal(JourneyUrls.ContinueToApplication(coordinator.InstanceId), continueLink.GetAttribute("href"));
            });

    [Fact]
    public Task Get_WithCustomAppContent_RendersCustomLinkTextAndReplacesPlaceholderWithUrl() =>
        WithJourneyCoordinatorAsync(
            (instanceId, processId) => CreateSignInJourneyState(
                instanceId,
                processId,
                "/",
                appContent: new AppContent
                {
                    OneLoginFoundPageLinkText = "<p class=\"govuk-body\">You can return to the <a href=\"{0}\" class=\"govuk-link\" data-testid=\"custom-link\">Register for a national professional qualification</a> service.</p>"
                }),
            async coordinator =>
            {
                // Arrange
                var person = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber());
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(person);

                await SetupInstanceStateAsync(coordinator, oneLoginUser, person.NationalInsuranceNumber!);

                var request = new HttpRequestMessage(HttpMethod.Get, JourneyUrls.Found(coordinator.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                var doc = await AssertEx.HtmlResponseAsync(response);
                var customLink = doc.GetElementByTestId("custom-link");
                Assert.NotNull(customLink);
                Assert.Contains("Register for a national professional qualification", customLink.TextContent);
                Assert.Equal(JourneyUrls.ContinueToApplication(coordinator.InstanceId), customLink.GetAttribute("href"));
            });

    private async Task SetupInstanceStateAsync(
        SignInJourneyCoordinator coordinator,
        OneLoginUser oneLoginUser,
        string nationalInsuranceNumber)
    {
        var ticket = CreateOneLoginAuthenticationTicket(vtr: AuthenticationOnly, oneLoginUser);
        await coordinator.OnOneLoginCallbackAsync(ticket);
        AddUrlToPath(coordinator, StepUrls.NationalInsuranceNumber);
        coordinator.UpdateState(s => s.SetNationalInsuranceNumber(true, nationalInsuranceNumber));
        var matched = await coordinator.TryMatchToTeachingRecordAsync();
        Debug.Assert(matched is not null);
        AddUrlToPath(coordinator, StepUrls.Found);
    }
}
