using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using static TeachingRecordSystem.AuthorizeAccess.SignInJourneyCoordinator.Vtrs;

namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests;

public class TrnTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public Task Get_ValidRequest_RendersExpectedContent(bool haveExistingValueInState) =>
        WithJourneyCoordinatorAsync(
            CreateNewState,
            async coordinator =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                await SetupInstanceStateAsync(coordinator, oneLoginUser);

                var existingTrn = haveExistingValueInState ? await TestData.GenerateTrnAsync() : null;

                coordinator.UpdateState(state =>
                {
                    if (existingTrn is not null)
                    {
                        state.SetTrn(true, existingTrn);
                    }
                });

                var request = new HttpRequestMessage(HttpMethod.Get, JourneyUrls.Trn(coordinator.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                var doc = await AssertEx.HtmlResponseAsync(response);
                Assert.Equal(existingTrn ?? "", doc.GetElementById("Trn")?.GetAttribute("value"));
            });

    [Fact]
    public Task Post_HaveTrnNotAnswered_RendersError() =>
        WithJourneyCoordinatorAsync(
            CreateNewState,
            async coordinator =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                await SetupInstanceStateAsync(coordinator, oneLoginUser);

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.Trn(coordinator.InstanceId))
                {
                    Content = new FormUrlEncodedContentBuilder { }
                };

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                await AssertEx.HtmlResponseHasErrorAsync(response, "HaveTrn", "Select yes if you have a teacher reference number");
            });

    [Fact]
    public Task Post_EmptyTrn_RendersError() =>
        WithJourneyCoordinatorAsync(
            CreateNewState,
            async coordinator =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                await SetupInstanceStateAsync(coordinator, oneLoginUser);

                var trn = "";

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.Trn(coordinator.InstanceId))
                {
                    Content = new FormUrlEncodedContentBuilder { { "HaveTrn", bool.TrueString }, { "Trn", trn } }
                };

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                await AssertEx.HtmlResponseHasErrorAsync(response, "Trn", "Enter your teacher reference number");
            });

    [Fact]
    public Task Post_InvalidTrn_RendersError() =>
        WithJourneyCoordinatorAsync(
            CreateNewState,
            async coordinator =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                await SetupInstanceStateAsync(coordinator, oneLoginUser);

                var trn = "xxx";

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.Trn(coordinator.InstanceId))
                {
                    Content = new FormUrlEncodedContentBuilder { { "HaveTrn", bool.TrueString }, { "Trn", trn } }
                };

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                await AssertEx.HtmlResponseHasErrorAsync(response, "Trn", "Your teacher reference number should contain 7 digits");
            });

    [Fact]
    public Task Post_TrnWithAllZeros_RendersError() =>
        WithJourneyCoordinatorAsync(
            CreateNewState,
            async coordinator =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                await SetupInstanceStateAsync(coordinator, oneLoginUser);

                var trn = "0000000";

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.Trn(coordinator.InstanceId))
                {
                    Content = new FormUrlEncodedContentBuilder { { "HaveTrn", bool.TrueString }, { "Trn", trn } }
                };

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                await AssertEx.HtmlResponseHasErrorAsync(response, "Trn", "Enter a valid teacher reference number");
            });

    [Fact]
    public Task Post_NoTrnSpecified_UpdatesStateAndRedirectsToNotFoundPage() =>
        WithJourneyCoordinatorAsync(
            CreateNewState,
            async coordinator =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                await SetupInstanceStateAsync(coordinator, oneLoginUser);

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.Trn(coordinator.InstanceId))
                {
                    Content = new FormUrlEncodedContentBuilder { { "HaveTrn", bool.FalseString } }
                };

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(JourneyUrls.NotFound(coordinator.InstanceId), response.Headers.Location?.OriginalString);

                var state = coordinator.State;
                Assert.False(state.HaveTrn);
                Assert.Null(state.Trn);
                Assert.Null(state.AuthenticationTicket);
            });

    [Fact]
    public Task Post_ValidTrnButLookupFailed_UpdatesStateAndRedirectsToNotFoundPage() =>
        WithJourneyCoordinatorAsync(
            CreateNewState,
            async coordinator =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                await SetupInstanceStateAsync(coordinator, oneLoginUser);

                var trn = await TestData.GenerateTrnAsync();

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.Trn(coordinator.InstanceId))
                {
                    Content = new FormUrlEncodedContentBuilder { { "HaveTrn", bool.TrueString }, { "Trn", trn } }
                };

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(JourneyUrls.NotFound(coordinator.InstanceId), response.Headers.Location?.OriginalString);

                var state = coordinator.State;
                Assert.True(state.HaveTrn);
                Assert.Equal(trn, state.Trn);
                Assert.Null(state.AuthenticationTicket);
            });

    [Fact]
    public Task Post_ValidTrnAndLookupSucceeded_UpdatesStateUpdatesOneLoginUserCompletesAuthenticationAndRedirectsToFoundPage() =>
        WithJourneyCoordinatorAsync(
            CreateNewState,
            async coordinator =>
            {
                // Arrange
                var person = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber());
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(
                    personId: null,
                    verifiedInfo: ([person.FirstName, person.LastName], person.DateOfBirth));

                await SetupInstanceStateAsync(coordinator, oneLoginUser);

                var trn = person.Trn;

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.Trn(coordinator.InstanceId))
                {
                    Content = new FormUrlEncodedContentBuilder { { "HaveTrn", bool.TrueString }, { "Trn", trn } }
                };

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(JourneyUrls.Found(coordinator.InstanceId), response.Headers.Location?.OriginalString);

                var state = coordinator.State;
                Assert.True(state.HaveTrn);
                Assert.Equal(trn, state.Trn);
                Assert.NotNull(state.AuthenticationTicket);

                oneLoginUser = await WithDbContextAsync(dbContext => dbContext.OneLoginUsers.SingleAsync(u => u.Subject == oneLoginUser.Subject));
                Assert.Equal(Clock.UtcNow, oneLoginUser.FirstSignIn);
                Assert.Equal(Clock.UtcNow, oneLoginUser.LastSignIn);
                Assert.Equal(person.PersonId, oneLoginUser.PersonId);
            });

    private async Task SetupInstanceStateAsync(
        SignInJourneyCoordinator coordinator,
        OneLoginUser oneLoginUser,
        string? nationalInsuranceNumber = null)
    {
        var ticket = CreateOneLoginAuthenticationTicket(vtr: AuthenticationOnly, oneLoginUser);
        await coordinator.OnOneLoginCallbackAsync(ticket);
        AddUrlToPath(coordinator, StepUrls.NationalInsuranceNumber);
        coordinator.UpdateState(s => s.SetNationalInsuranceNumber(true, nationalInsuranceNumber ?? TestData.GenerateNationalInsuranceNumber()));
        AddUrlToPath(coordinator, StepUrls.Trn);
    }
}
