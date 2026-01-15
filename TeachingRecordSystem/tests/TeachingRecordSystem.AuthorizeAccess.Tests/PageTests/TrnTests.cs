using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using static TeachingRecordSystem.AuthorizeAccess.SignInJourneyCoordinator.Vtrs;

namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests;

public class TrnTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public Task Get_NotAuthenticatedWithOneLogin_ReturnsBadRequest() =>
        WithJourneyInstanceAsync(
            CreateNewState(),
            async journeyInstance =>
            {
                // Arrange
                var request = new HttpRequestMessage(HttpMethod.Get, JourneyUrls.Trn(journeyInstance.InstanceId));

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

                var request = new HttpRequestMessage(HttpMethod.Get, JourneyUrls.Trn(journeyInstance.InstanceId));

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

                var request = new HttpRequestMessage(HttpMethod.Get, JourneyUrls.Trn(journeyInstance.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(journeyInstance.State.RedirectUri, response.Headers.Location?.OriginalString);
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

                var request = new HttpRequestMessage(HttpMethod.Get, JourneyUrls.Trn(journeyInstance.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(JourneyUrls.NationalInsuranceNumber(journeyInstance.InstanceId), response.Headers.Location?.OriginalString);
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

                var existingTrn = haveExistingValueInState ? await TestData.GenerateTrnAsync() : null;
                if (existingTrn is not null)
                {
                    journeyInstance.UpdateState(s => s.SetTrn(true, existingTrn));
                }

                var request = new HttpRequestMessage(HttpMethod.Get, JourneyUrls.Trn(journeyInstance.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                var doc = await AssertEx.HtmlResponseAsync(response);
                Assert.Equal(existingTrn ?? "", doc.GetElementById("Trn")?.GetAttribute("value"));
            });
    [Fact]
    public Task Post_NotAuthenticatedWithOneLogin_ReturnsBadRequest() =>
        WithJourneyInstanceAsync(
            CreateNewState(),
            async journeyInstance =>
            {
                // Arrange
                var trn = await TestData.GenerateTrnAsync();

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.Trn(journeyInstance.InstanceId))
                {
                    Content = new FormUrlEncodedContentBuilder
                    {
                        { "HaveTrn", bool.TrueString },
                        { "Trn", trn }
                    }
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
                var trn = await TestData.GenerateTrnAsync();

                var ticket = CreateOneLoginAuthenticationTicket(vtr: AuthenticationOnly, createCoreIdentityVc: false);
                await journeyInstance.OnOneLoginCallbackAsync(ticket);

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.Trn(journeyInstance.InstanceId))
                {
                    Content = new FormUrlEncodedContentBuilder
                    {
                        { "HaveTrn", bool.TrueString },
                        { "Trn", trn }
                    }
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

                var trn = person.Trn;

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.Trn(journeyInstance.InstanceId))
                {
                    Content = new FormUrlEncodedContentBuilder
                    {
                        { "HaveTrn", bool.TrueString },
                        { "Trn", trn }
                    }
                };

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(journeyInstance.State.RedirectUri, response.Headers.Location?.OriginalString);
            });

    [Fact]
    public Task Post_NationalInsuranceNumberNotSpecified_RedirectsToNationalInsuranceNumberPage() =>
        WithJourneyInstanceAsync(
            CreateNewState(),
            async journeyInstance =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                var ticket = CreateOneLoginAuthenticationTicket(vtr: AuthenticationOnly, oneLoginUser);
                await journeyInstance.OnOneLoginCallbackAsync(ticket);

                var trn = await TestData.GenerateTrnAsync();

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.Trn(journeyInstance.InstanceId))
                {
                    Content = new FormUrlEncodedContentBuilder
                    {
                        { "HaveTrn", bool.TrueString },
                        { "Trn", trn }
                    }
                };

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(JourneyUrls.NationalInsuranceNumber(journeyInstance.InstanceId), response.Headers.Location?.OriginalString);
            });

    [Fact]
    public Task Post_HaveTrnNotAnswered_RendersError() =>
        WithJourneyInstanceAsync(
            CreateNewState(),
            async journeyInstance =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                await SetupInstanceStateAsync(journeyInstance, oneLoginUser);

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.Trn(journeyInstance.InstanceId))
                {
                    Content = new FormUrlEncodedContentBuilder
                    {
                    }
                };

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                await AssertEx.HtmlResponseHasErrorAsync(response, "HaveTrn", "Select yes if you have a teacher reference number");
            });
    [Fact]
    public Task Post_EmptyTrn_RendersError() =>
        WithJourneyInstanceAsync(
            CreateNewState(),
            async journeyInstance =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                await SetupInstanceStateAsync(journeyInstance, oneLoginUser);

                var trn = "";

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.Trn(journeyInstance.InstanceId))
                {
                    Content = new FormUrlEncodedContentBuilder
                    {
                        { "HaveTrn", bool.TrueString },
                        { "Trn", trn }
                    }
                };

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                await AssertEx.HtmlResponseHasErrorAsync(response, "Trn", "Enter your teacher reference number");
            });

    [Fact]
    public Task Post_InvalidTrn_RendersError() =>
        WithJourneyInstanceAsync(
            CreateNewState(),
            async journeyInstance =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                await SetupInstanceStateAsync(journeyInstance, oneLoginUser);

                var trn = "xxx";

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.Trn(journeyInstance.InstanceId))
                {
                    Content = new FormUrlEncodedContentBuilder
                    {
                        { "HaveTrn", bool.TrueString },
                        { "Trn", trn }
                    }
                };

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                await AssertEx.HtmlResponseHasErrorAsync(response, "Trn", "Your teacher reference number should contain 7 digits");
            });
    [Fact]
    public Task Post_NoTrnSpecified_UpdatesStateAndRedirectsToNotFoundPage() =>
        WithJourneyInstanceAsync(
            CreateNewState(),
            async journeyInstance =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                await SetupInstanceStateAsync(journeyInstance, oneLoginUser);

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.Trn(journeyInstance.InstanceId))
                {
                    Content = new FormUrlEncodedContentBuilder
                    {
                        { "HaveTrn", bool.FalseString }
                    }
                };

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(JourneyUrls.NotFound(journeyInstance.InstanceId), response.Headers.Location?.OriginalString);

                var state = journeyInstance.State;
                Assert.False(state.HaveTrn);
                Assert.Null(state.Trn);
                Assert.Null(state.AuthenticationTicket);
            });

    [Fact]
    public Task Post_ValidTrnButLookupFailed_UpdatesStateAndRedirectsToNotFoundPage() =>
        WithJourneyInstanceAsync(
            CreateNewState(),
            async journeyInstance =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                await SetupInstanceStateAsync(journeyInstance, oneLoginUser);

                var trn = await TestData.GenerateTrnAsync();

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.Trn(journeyInstance.InstanceId))
                {
                    Content = new FormUrlEncodedContentBuilder
                    {
                        { "HaveTrn", bool.TrueString },
                        { "Trn", trn }
                    }
                };

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(JourneyUrls.NotFound(journeyInstance.InstanceId), response.Headers.Location?.OriginalString);

                var state = journeyInstance.State;
                Assert.True(state.HaveTrn);
                Assert.Equal(trn, state.Trn);
                Assert.Null(state.AuthenticationTicket);
            });
    [Fact]
    public Task Post_ValidTrnAndLookupSucceeded_UpdatesStateUpdatesOneLoginUserCompletesAuthenticationAndRedirectsToFoundPage() =>
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

                var trn = person.Trn;

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.Trn(journeyInstance.InstanceId))
                {
                    Content = new FormUrlEncodedContentBuilder
                    {
                        { "HaveTrn", bool.TrueString },
                        { "Trn", trn }
                    }
                };

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(JourneyUrls.Found(journeyInstance.InstanceId), response.Headers.Location?.OriginalString);

                var state = journeyInstance.State;
                Assert.True(state.HaveTrn);
                Assert.Equal(trn, state.Trn);
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
        journeyInstance.UpdateState(s => s.SetNationalInsuranceNumber(true, Faker.Identification.UkNationalInsuranceNumber()));
        AddUrlToPath(journeyInstance, StepUrls.Trn);
    }
}
