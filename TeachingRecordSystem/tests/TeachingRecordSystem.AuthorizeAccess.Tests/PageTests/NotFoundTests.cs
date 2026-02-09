using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using static TeachingRecordSystem.AuthorizeAccess.SignInJourneyCoordinator.Vtrs;

namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests;

public class NotFoundTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public Task Get_ValidRequest_RendersExpectedContent() =>
        WithJourneyCoordinatorAsync(
            CreateSignInJourneyState,
            async coordinator =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                await SetupInstanceStateAsync(coordinator, oneLoginUser);

                await coordinator.UpdateStateAsync(async state =>
                {
                    state.SetNationalInsuranceNumber(true, TestData.GenerateNationalInsuranceNumber());
                    state.SetTrn(true, await TestData.GenerateTrnAsync());
                });

                var request = new HttpRequestMessage(HttpMethod.Get, JourneyUrls.NotFound(coordinator.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                await AssertEx.HtmlResponseAsync(response);
            });

    [Fact]
    public Task Post_ValidRequest_RedirectsToCheckAnswersPage() =>
        WithJourneyCoordinatorAsync(
            CreateSignInJourneyState,
            async coordinator =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                await SetupInstanceStateAsync(coordinator, oneLoginUser);

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.NotFound(coordinator.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(JourneyUrls.CheckAnswers(coordinator.InstanceId), response.Headers.Location?.OriginalString);
            });

    private async Task SetupInstanceStateAsync(
        SignInJourneyCoordinator coordinator,
        OneLoginUser oneLoginUser,
        string? nationalInsuranceNumber = null,
        string? trn = null)
    {
        var ticket = CreateOneLoginAuthenticationTicket(vtr: AuthenticationOnly, oneLoginUser);
        await coordinator.OnOneLoginCallbackAsync(ticket);
        AddUrlToPath(coordinator, StepUrls.NationalInsuranceNumber);
        coordinator.UpdateState(s => s.SetNationalInsuranceNumber(true, nationalInsuranceNumber ?? TestData.GenerateNationalInsuranceNumber()));
        AddUrlToPath(coordinator, StepUrls.Trn);
        await coordinator.UpdateStateAsync(async s => s.SetTrn(true, trn ?? await TestData.GenerateTrnAsync()));
        AddUrlToPath(coordinator, StepUrls.NotFound);
    }
}
