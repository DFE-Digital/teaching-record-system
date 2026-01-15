using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using static TeachingRecordSystem.AuthorizeAccess.SignInJourneyCoordinator.Vtrs;

namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests;

public class SupportRequestSubmittedTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public Task Get_NotAuthenticatedWithOneLogin_ReturnsBadRequest() =>
        WithJourneyInstanceAsync(
            CreateNewState(),
            async journeyInstance =>
            {
                // Arrange
                var request = new HttpRequestMessage(HttpMethod.Get, JourneyUrls.SupportRequestSubmitted(journeyInstance.InstanceId));

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
                // Arrange
                var ticket = CreateOneLoginAuthenticationTicket(vtr: AuthenticationOnly, createCoreIdentityVc: false);
                await journeyInstance.OnOneLoginCallbackAsync(ticket);

                var request = new HttpRequestMessage(HttpMethod.Get, JourneyUrls.SupportRequestSubmitted(journeyInstance.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
            });

    [Fact]
    public Task Get_NationalInsuranceNumberNotSpecified_RedirectsToNationalInsuranceNumberPage() =>
        WithJourneyInstanceAsync(
            CreateNewState(),
            async journeyInstance =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                var ticket = CreateOneLoginAuthenticationTicket(vtr: AuthenticationOnly, oneLoginUser);
                await journeyInstance.OnOneLoginCallbackAsync(ticket);

                var request = new HttpRequestMessage(HttpMethod.Get, JourneyUrls.SupportRequestSubmitted(journeyInstance.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(JourneyUrls.NationalInsuranceNumber(journeyInstance.InstanceId), response.Headers.Location?.OriginalString);
            });

    [Fact]
    public Task Get_TrnNotSpecified_RedirectsToTrnPage() =>
        WithJourneyInstanceAsync(
            CreateNewState(),
            async journeyInstance =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                var ticket = CreateOneLoginAuthenticationTicket(vtr: AuthenticationOnly, oneLoginUser);
                await journeyInstance.OnOneLoginCallbackAsync(ticket);

                journeyInstance.UpdateState(state => state.SetNationalInsuranceNumber(true, Faker.Identification.UkNationalInsuranceNumber()));

                var request = new HttpRequestMessage(HttpMethod.Get, JourneyUrls.SupportRequestSubmitted(journeyInstance.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(JourneyUrls.Trn(journeyInstance.InstanceId), response.Headers.Location?.OriginalString);
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

                var request = new HttpRequestMessage(HttpMethod.Get, JourneyUrls.SupportRequestSubmitted(journeyInstance.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(journeyInstance.State.RedirectUri, response.Headers.Location?.OriginalString);
            });

    [Fact]
    public Task Get_SupportTicketNotCreated_RedirectsToCheckAnswersPage() =>
        WithJourneyInstanceAsync(
            CreateNewState(),
            async journeyInstance =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                var ticket = CreateOneLoginAuthenticationTicket(vtr: AuthenticationOnly, oneLoginUser);
                await journeyInstance.OnOneLoginCallbackAsync(ticket);

                var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();
                var trn = await TestData.GenerateTrnAsync();

                journeyInstance.UpdateState(state =>
                {
                    state.SetNationalInsuranceNumber(true, nationalInsuranceNumber);
                    state.SetTrn(true, trn);
                });

                var request = new HttpRequestMessage(HttpMethod.Get, JourneyUrls.SupportRequestSubmitted(journeyInstance.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(JourneyUrls.CheckAnswers(journeyInstance.InstanceId), response.Headers.Location?.OriginalString);
            });

    [Fact]
    public Task Get_ValidRequest_ReturnsExpectedContent() =>
        WithJourneyInstanceAsync(
            CreateNewState(),
            async journeyInstance =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                var ticket = CreateOneLoginAuthenticationTicket(vtr: AuthenticationOnly, oneLoginUser);
                await journeyInstance.OnOneLoginCallbackAsync(ticket);

                var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();
                var trn = await TestData.GenerateTrnAsync();

                journeyInstance.UpdateState(state =>
                {
                    state.SetNationalInsuranceNumber(true, nationalInsuranceNumber);
                    state.SetTrn(true, trn);
                    state.HasPendingSupportRequest = true;
                });

                var request = new HttpRequestMessage(HttpMethod.Get, JourneyUrls.SupportRequestSubmitted(journeyInstance.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                await AssertEx.HtmlResponseAsync(response);
            });
}
