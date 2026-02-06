using GovUk.Questions.AspNetCore;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using static TeachingRecordSystem.AuthorizeAccess.SignInJourneyCoordinator.Vtrs;

namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests;

public class SupportRequestSubmittedTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public Task Get_ValidRequest_ReturnsExpectedContent() =>
        WithJourneyCoordinatorAsync(
            CreateSignInJourneyState,
            async coordinator =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                await SetupInstanceStateAsync(coordinator, oneLoginUser);

                var request = new HttpRequestMessage(HttpMethod.Get, JourneyUrls.RequestSubmitted(coordinator.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                await AssertEx.HtmlResponseAsync(response);
            });

    private async Task SetupInstanceStateAsync(
        SignInJourneyCoordinator coordinator,
        OneLoginUser oneLoginUser,
        string? nationalInsuranceNumber = null,
        string? trn = null)
    {
        var ticket = CreateOneLoginAuthenticationTicket(vtr: AuthenticationOnly, oneLoginUser);
        await coordinator.OnOneLoginCallbackAsync(ticket);
        await coordinator.UpdateStateAsync(async s =>
        {
            s.SetNationalInsuranceNumber(true, nationalInsuranceNumber ?? TestData.GenerateNationalInsuranceNumber());
            s.SetTrn(true, trn ?? await TestData.GenerateTrnAsync());
        });
        coordinator.UnsafeSetPath(new JourneyPath([coordinator.CreateStepFromUrl(StepUrls.RequestSubmitted)]));
    }
}
