using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using static TeachingRecordSystem.AuthorizeAccess.SignInJourneyCoordinator.Vtrs;

namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests;

public class NationalInsuranceNumberTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public Task Get_NotAuthenticatedWithOneLogin_ReturnsBadRequest() =>
        WithJourneyInstanceAsync(
            CreateNewState(),
            async journeyInstance =>
            {
                // Arrange
                var request = new HttpRequestMessage(HttpMethod.Get, JourneyUrls.NationalInsuranceNumber(journeyInstance.InstanceId));

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

                var request = new HttpRequestMessage(HttpMethod.Get, JourneyUrls.NationalInsuranceNumber(journeyInstance.InstanceId));

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

                var request = new HttpRequestMessage(HttpMethod.Get, JourneyUrls.NationalInsuranceNumber(journeyInstance.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(journeyInstance.State.RedirectUri, response.Headers.Location?.OriginalString);
            });

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public Task Get_ValidRequest_RendersExpectedContent(bool haveExistingValueInState) =>
        WithJourneyInstanceAsync(
            CreateNewState(),
            async journeyInstance =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                await SetupInstanceStateAsync(journeyInstance, oneLoginUser);

                var existingNationalInsuranceNumber = haveExistingValueInState ? Faker.Identification.UkNationalInsuranceNumber() : null;
                if (existingNationalInsuranceNumber is not null)
                {
                    journeyInstance.UpdateState(s => s.SetNationalInsuranceNumber(true, existingNationalInsuranceNumber));
                }

                var request = new HttpRequestMessage(HttpMethod.Get, JourneyUrls.NationalInsuranceNumber(journeyInstance.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                var doc = await AssertEx.HtmlResponseAsync(response);
                Assert.Equal(existingNationalInsuranceNumber ?? "", doc.GetElementById("NationalInsuranceNumber")?.GetAttribute("value"));
            });

    [Fact]
    public Task Post_NotAuthenticatedWithOneLogin_ReturnsBadRequest() =>
        WithJourneyInstanceAsync(
            CreateNewState(),
            async journeyInstance =>
            {
                // Arrange
                var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.NationalInsuranceNumber(journeyInstance.InstanceId))
                {
                    Content = new FormUrlEncodedContentBuilder { { "NationalInsuranceNumber", nationalInsuranceNumber } }
                };

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

                var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.NationalInsuranceNumber(journeyInstance.InstanceId))
                {
                    Content = new FormUrlEncodedContentBuilder { { "NationalInsuranceNumber", nationalInsuranceNumber } }
                };

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

                var nationalInsuranceNumber = person.NationalInsuranceNumber!;

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.NationalInsuranceNumber(journeyInstance.InstanceId))
                {
                    Content = new FormUrlEncodedContentBuilder { { "NationalInsuranceNumber", nationalInsuranceNumber } }
                };

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(journeyInstance.State.RedirectUri, response.Headers.Location?.OriginalString);
            });

    [Fact]
    public Task Post_HaveNationalInsuranceNumberNotAnswered_RendersError() =>
        WithJourneyInstanceAsync(
            CreateNewState(),
            async journeyInstance =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                await SetupInstanceStateAsync(journeyInstance, oneLoginUser);

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.NationalInsuranceNumber(journeyInstance.InstanceId))
                {
                    Content = new FormUrlEncodedContentBuilder { }
                };

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                await AssertEx.HtmlResponseHasErrorAsync(response, "HaveNationalInsuranceNumber", "Select yes if you have a National Insurance number");
            });

    [Fact]
    public Task Post_EmptyNationalInsuranceNumber_RendersError() =>
        WithJourneyInstanceAsync(
            CreateNewState(),
            async journeyInstance =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                await SetupInstanceStateAsync(journeyInstance, oneLoginUser);

                var nationalInsuranceNumber = "";

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.NationalInsuranceNumber(journeyInstance.InstanceId))
                {
                    Content = new FormUrlEncodedContentBuilder
                    {
                        { "HaveNationalInsuranceNumber", bool.TrueString }, { "NationalInsuranceNumber", nationalInsuranceNumber }
                    }
                };

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                await AssertEx.HtmlResponseHasErrorAsync(response, "NationalInsuranceNumber", "Enter a National Insurance number");
            });

    [Fact]
    public Task Post_InvalidNationalInsuranceNumber_RendersError() =>
        WithJourneyInstanceAsync(
            CreateNewState(),
            async journeyInstance =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                await SetupInstanceStateAsync(journeyInstance, oneLoginUser);

                var nationalInsuranceNumber = "xxx";

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.NationalInsuranceNumber(journeyInstance.InstanceId))
                {
                    Content = new FormUrlEncodedContentBuilder
                    {
                        { "HaveNationalInsuranceNumber", bool.TrueString }, { "NationalInsuranceNumber", nationalInsuranceNumber }
                    }
                };

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                await AssertEx.HtmlResponseHasErrorAsync(response, "NationalInsuranceNumber", "Enter a National Insurance number in the correct format");
            });

    [Fact]
    public Task Post_HaveNationalInsuranceNumberNotSpecified_UpdatesStateAndRedirectsToTrnPage() =>
        WithJourneyInstanceAsync(
            CreateNewState(),
            async journeyInstance =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                await SetupInstanceStateAsync(journeyInstance, oneLoginUser);

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.NationalInsuranceNumber(journeyInstance.InstanceId))
                {
                    Content = new FormUrlEncodedContentBuilder { { "HaveNationalInsuranceNumber", bool.FalseString } }
                };

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(JourneyUrls.Trn(journeyInstance.InstanceId), response.Headers.Location?.OriginalString);

                var state = journeyInstance.State;
                Assert.False(state.HaveNationalInsuranceNumber);
                Assert.Null(state.AuthenticationTicket);
            });

    [Fact]
    public Task Post_ValidNationalInsuranceNumberButLookupFailed_UpdatesStateAndRedirectsToTrnPage() =>
        WithJourneyInstanceAsync(
            CreateNewState(),
            async journeyInstance =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                await SetupInstanceStateAsync(journeyInstance, oneLoginUser);

                var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.NationalInsuranceNumber(journeyInstance.InstanceId))
                {
                    Content = new FormUrlEncodedContentBuilder
                    {
                        { "HaveNationalInsuranceNumber", bool.TrueString }, { "NationalInsuranceNumber", nationalInsuranceNumber }
                    }
                };

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(JourneyUrls.Trn(journeyInstance.InstanceId), response.Headers.Location?.OriginalString);

                var state = journeyInstance.State;
                Assert.True(state.HaveNationalInsuranceNumber);
                Assert.Equal(nationalInsuranceNumber, state.NationalInsuranceNumber);
                Assert.Null(state.AuthenticationTicket);
            });

    [Fact]
    public Task Post_ValidNationalInsuranceNumberAndLookupSucceeded_UpdatesStateUpdatesOneLoginUserCompletesAuthenticationAndRedirectsToFoundPage() =>
        WithJourneyInstanceAsync(
            CreateNewState(),
            async journeyInstance =>
            {
                // Arrange
                var person = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber());
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(
                    personId: null,
                    verifiedInfo: ([person.FirstName, person.LastName], person.DateOfBirth));

                await SetupInstanceStateAsync(journeyInstance, oneLoginUser);

                var nationalInsuranceNumber = person.NationalInsuranceNumber!;

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.NationalInsuranceNumber(journeyInstance.InstanceId))
                {
                    Content = new FormUrlEncodedContentBuilder
                    {
                        { "HaveNationalInsuranceNumber", bool.TrueString }, { "NationalInsuranceNumber", nationalInsuranceNumber }
                    }
                };

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(JourneyUrls.Found(journeyInstance.InstanceId), response.Headers.Location?.OriginalString);

                var state = journeyInstance.State;
                Assert.True(state.HaveNationalInsuranceNumber);
                Assert.Equal(nationalInsuranceNumber, state.NationalInsuranceNumber);
                Assert.NotNull(state.AuthenticationTicket);

                oneLoginUser = await WithDbContextAsync(dbContext => dbContext.OneLoginUsers.SingleAsync(u => u.Subject == oneLoginUser.Subject));
                Assert.Equal(Clock.UtcNow, oneLoginUser.FirstSignIn);
                Assert.Equal(Clock.UtcNow, oneLoginUser.LastSignIn);
                Assert.Equal(person.PersonId, oneLoginUser.PersonId);
            });

    private async Task SetupInstanceStateAsync(SignInJourneyCoordinator journeyInstance, OneLoginUser oneLoginUser)
    {
        var ticket = CreateOneLoginAuthenticationTicket(vtr: AuthenticationOnly, oneLoginUser);
        await journeyInstance.OnOneLoginCallbackAsync(ticket);
        AddUrlToPath(journeyInstance, StepUrls.NationalInsuranceNumber);
    }
}
