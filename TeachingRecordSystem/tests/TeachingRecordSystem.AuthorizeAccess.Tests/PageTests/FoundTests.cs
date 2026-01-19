using System.Diagnostics;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using static TeachingRecordSystem.AuthorizeAccess.SignInJourneyCoordinator.Vtrs;

namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests;

public class FoundTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public Task Get_ValidRequest_RendersExpectedContent() =>
        WithJourneyCoordinatorAsync(
            CreateNewState,
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
    public Task Post_ValidRequest_RedirectsToStateRedirectUri() =>
        WithJourneyCoordinatorAsync(
            CreateNewState,
            async coordinator =>
            {
                // Arrange
                var person = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber());
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(person);

                await SetupInstanceStateAsync(coordinator, oneLoginUser, person.NationalInsuranceNumber!);

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.Found(coordinator.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(coordinator.GetRedirectUri(), response.Headers.Location?.OriginalString);
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
