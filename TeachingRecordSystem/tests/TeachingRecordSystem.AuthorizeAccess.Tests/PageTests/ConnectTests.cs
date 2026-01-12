using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using static TeachingRecordSystem.AuthorizeAccess.SignInJourneyCoordinator.Vtrs;

namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests;

public class ConnectTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public Task Get_NotAuthenticatedWithOneLogin_ReturnsBadRequest() =>
        WithJourneyInstanceAsync(
            CreateNewState(),
            async journeyInstance =>
            {
                // Arrange
                var request = new HttpRequestMessage(HttpMethod.Get, JourneyUrls.Connect(journeyInstance.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
            });

    [Fact]
    public Task Get_NotVerifiedWithOneLogin_ReturnsBadRequest() =>
        WithJourneyInstanceAsync(
            CreateNewState(),
            async journeyInstance =>
            {
                var ticket = CreateOneLoginAuthenticationTicket(vtr: AuthenticationOnly, createCoreIdentityVc: false);
                await journeyInstance.OnOneLoginCallbackAsync(ticket);

                var request = new HttpRequestMessage(HttpMethod.Get, JourneyUrls.Connect(journeyInstance.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
            });

    [Fact]
    public Task Get_AlreadyAuthenticated_RedirectsToStateRedirectUri() =>
        WithJourneyInstanceAsync(
            CreateNewState(),
            async journeyInstance =>
            {
                // Arrange
                var person = await TestData.CreatePersonAsync();
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(person);

                var ticket = CreateOneLoginAuthenticationTicket(vtr: AuthenticationOnly, oneLoginUser);
                await journeyInstance.OnOneLoginCallbackAsync(ticket);

                var request = new HttpRequestMessage(HttpMethod.Get, JourneyUrls.Connect(journeyInstance.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(journeyInstance.State.RedirectUri, response.Headers.Location?.OriginalString);
            });

    [Fact]
    public Task Get_ValidRequest_RendersExpectedContent() =>
        WithJourneyInstanceAsync(
            CreateNewState(),
            async journeyInstance =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                await SetupInstanceStateAsync(journeyInstance, oneLoginUser);

                var request = new HttpRequestMessage(HttpMethod.Get, JourneyUrls.Connect(journeyInstance.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                await AssertEx.HtmlResponseAsync(response);
            });

    [Fact]
    public Task Post_NotAuthenticatedWithOneLogin_ReturnsBadRequest() =>
        WithJourneyInstanceAsync(
            CreateNewState(),
            async journeyInstance =>
            {
                // Arrange
                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.Connect(journeyInstance.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
            });

    [Fact]
    public Task Post_NotVerifiedWithOneLogin_ReturnsBadRequest() =>
        WithJourneyInstanceAsync(
            CreateNewState(),
            async journeyInstance =>
            {
                // Arrange
                var ticket = CreateOneLoginAuthenticationTicket(vtr: AuthenticationOnly, createCoreIdentityVc: false);
                await journeyInstance.OnOneLoginCallbackAsync(ticket);

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.Connect(journeyInstance.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
            });

    [Fact]
    public Task Post_AlreadyAuthenticated_RedirectsToStateRedirectUri() =>
        WithJourneyInstanceAsync(
            CreateNewState(),
            async journeyInstance =>
            {
                // Arrange
                var person = await TestData.CreatePersonAsync();
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(person);

                var ticket = CreateOneLoginAuthenticationTicket(vtr: AuthenticationOnly, oneLoginUser);
                await journeyInstance.OnOneLoginCallbackAsync(ticket);

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.Connect(journeyInstance.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(journeyInstance.State.RedirectUri, response.Headers.Location?.OriginalString);
            });

    [Fact]
    public Task Post_ValidRequest_RedirectsToStartOfMatchingJourney() =>
        WithJourneyInstanceAsync(
            CreateNewState(),
            async journeyInstance =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                await SetupInstanceStateAsync(journeyInstance, oneLoginUser);

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.Connect(journeyInstance.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(JourneyUrls.NationalInsuranceNumber(journeyInstance.InstanceId), response.Headers.Location?.OriginalString);
            });

    private async Task SetupInstanceStateAsync(SignInJourneyCoordinator journeyInstance, OneLoginUser oneLoginUser)
    {
        var ticket = CreateOneLoginAuthenticationTicket(vtr: AuthenticationOnly, oneLoginUser);
        await journeyInstance.OnOneLoginCallbackAsync(ticket);
    }
}
