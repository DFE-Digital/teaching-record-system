using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using static TeachingRecordSystem.AuthorizeAccess.SignInJourneyCoordinator.Vtrs;

namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests;

public class NotVerifiedTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public Task Get_NotAuthenticatedWithOneLogin_ReturnsBadRequest() =>
        WithJourneyInstanceAsync(
            CreateNewState(),
            async journeyInstance =>
            {
                // Arrange
                var request = new HttpRequestMessage(HttpMethod.Get, JourneyUrls.NotVerified(journeyInstance.InstanceId));

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
                var person = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber());
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(person);

                var ticket = CreateOneLoginAuthenticationTicket(vtr: AuthenticationOnly, oneLoginUser);
                await journeyInstance.OnOneLoginCallbackAsync(ticket);

                var request = new HttpRequestMessage(HttpMethod.Get, JourneyUrls.NotVerified(journeyInstance.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(journeyInstance.State.RedirectUri, response.Headers.Location?.OriginalString);
            });

    [Fact]
    public Task Get_VerifiedWithOneLogin_ReturnsBadRequest() =>
        WithJourneyInstanceAsync(
            CreateNewState(),
            async journeyInstance =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                var ticket = CreateOneLoginAuthenticationTicket(vtr: AuthenticationOnly, oneLoginUser);
                await journeyInstance.OnOneLoginCallbackAsync(ticket);

                var request = new HttpRequestMessage(HttpMethod.Get, JourneyUrls.NotVerified(journeyInstance.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
            });

    [Fact]
    public Task Get_ValidRequest_ReturnsExpectedContent() =>
        WithJourneyInstanceAsync(
            CreateNewState(),
            async journeyInstance =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);

                var ticket = CreateOneLoginAuthenticationTicket(vtr: AuthenticationOnly, oneLoginUser);
                await journeyInstance.OnOneLoginCallbackAsync(ticket);

                var request = new HttpRequestMessage(HttpMethod.Get, JourneyUrls.NotVerified(journeyInstance.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                await AssertEx.HtmlResponseAsync(response);
            });
}
