using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using static TeachingRecordSystem.AuthorizeAccess.SignInJourneyCoordinator.Vtrs;

namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests;

public class QtsStatusTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public Task Get_ValidRequest_RendersExpectedContent(bool haveExistingValueInState) =>
        WithJourneyCoordinatorAsync(
            CreateSignInJourneyState,
            async coordinator =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                await SetupInstanceStateAsync(coordinator, oneLoginUser);

                if (haveExistingValueInState)
                {
                    coordinator.UpdateState(s => s.SetQts(true));
                }

                var request = new HttpRequestMessage(HttpMethod.Get, JourneyUrls.QtsStatus(coordinator.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                await AssertEx.HtmlResponseAsync(response);
            });

    [Fact]
    public Task Post_HaveQtsNotAnswered_RendersError() =>
        WithJourneyCoordinatorAsync(
            CreateSignInJourneyState,
            async coordinator =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                await SetupInstanceStateAsync(coordinator, oneLoginUser);

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.QtsStatus(coordinator.InstanceId))
                {
                    Content = new FormUrlEncodedContentBuilder { }
                };

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                await AssertEx.HtmlResponseHasErrorAsync(response, "HaveQts", "Select yes if you have qualified teacher status (QTS)");
            });

    [Fact]
    public Task Post_HaveQtsNo_UpdatesStateAndRedirectsToCheckAnswersPage() =>
        WithJourneyCoordinatorAsync(
            CreateSignInJourneyState,
            async coordinator =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                await SetupInstanceStateAsync(coordinator, oneLoginUser);

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.QtsStatus(coordinator.InstanceId))
                {
                    Content = new FormUrlEncodedContentBuilder { { "HaveQts", bool.FalseString } }
                };

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(JourneyUrls.CheckAnswers(coordinator.InstanceId), response.Headers.Location?.OriginalString);

                var state = coordinator.State;
                Assert.False(state.HaveQts);
            });

    [Fact]
    public Task Post_HaveQtsYes_UpdatesStateAndRedirectsToQtsDetailsPage() =>
        WithJourneyCoordinatorAsync(
            CreateSignInJourneyState,
            async coordinator =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                await SetupInstanceStateAsync(coordinator, oneLoginUser);

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.QtsStatus(coordinator.InstanceId))
                {
                    Content = new FormUrlEncodedContentBuilder { { "HaveQts", bool.TrueString } }
                };

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(JourneyUrls.QtsDetails(coordinator.InstanceId), response.Headers.Location?.OriginalString);

                var state = coordinator.State;
                Assert.True(state.HaveQts);
            });

    private async Task SetupInstanceStateAsync(SignInJourneyCoordinator coordinator, OneLoginUser oneLoginUser)
    {
        var ticket = CreateOneLoginAuthenticationTicket(vtr: AuthenticationOnly, oneLoginUser);
        await coordinator.OnOneLoginCallbackAsync(ticket);
        AddUrlToPath(coordinator, StepUrls.NotFound);
        AddUrlToPath(coordinator, StepUrls.QtsStatus);
    }
}
