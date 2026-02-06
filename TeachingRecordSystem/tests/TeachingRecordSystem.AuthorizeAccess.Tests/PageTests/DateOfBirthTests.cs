using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using static TeachingRecordSystem.AuthorizeAccess.SignInJourneyCoordinator.Vtrs;

namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests;

public class DateOfBirthTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public Task Get_ValidRequest_ReturnsExpectedContent(bool haveExistingValueInState) =>
        WithJourneyCoordinatorAsync(
            CreateSignInJourneyState,
            async coordinator =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);

                await SetupInstanceStateAsync(coordinator, oneLoginUser);

                var existingDateOfBirth = haveExistingValueInState ? new DateOnly(1980, 3, 31) : (DateOnly?)null;

                if (haveExistingValueInState)
                {
                    coordinator.UpdateState(state => state.SetDateOfBirth(existingDateOfBirth!.Value));
                }

                var request = new HttpRequestMessage(HttpMethod.Get, JourneyUrls.DateOfBirth(coordinator.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                var doc = await AssertEx.HtmlResponseAsync(response);
                if (haveExistingValueInState)
                {
                    Assert.Equal("31", doc.GetElementById("DateOfBirth.Day")?.GetAttribute("value"));
                    Assert.Equal("3", doc.GetElementById("DateOfBirth.Month")?.GetAttribute("value"));
                    Assert.Equal("1980", doc.GetElementById("DateOfBirth.Year")?.GetAttribute("value"));
                }
                else
                {
                    Assert.Null(doc.GetElementById("DateOfBirth.Day")?.GetAttribute("value"));
                    Assert.Null(doc.GetElementById("DateOfBirth.Month")?.GetAttribute("value"));
                    Assert.Null(doc.GetElementById("DateOfBirth.Year")?.GetAttribute("value"));
                }
            });

    [Fact]
    public Task Post_InvalidRequest_ShowsExpectedError() =>
        WithJourneyCoordinatorAsync(
            CreateSignInJourneyState,
            async coordinator =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);

                await SetupInstanceStateAsync(coordinator, oneLoginUser);

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.DateOfBirth(coordinator.InstanceId))
                {
                    Content = new FormUrlEncodedContent(new Dictionary<string, string?>
                    {
                        { "DateOfBirth.Day", "" },
                        { "DateOfBirth.Month", "" },
                        { "DateOfBirth.Year", "" }
                    })
                };

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                await AssertEx.HtmlResponseHasErrorAsync(response, "DateOfBirth", "Enter your date of birth");
            });

    [Fact]
    public Task Post_ValidRequest_UpdatesStateAndRedirectsToNationalInsuranceNumberPage() =>
        WithJourneyCoordinatorAsync(
            CreateSignInJourneyState,
            async coordinator =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);

                await SetupInstanceStateAsync(coordinator, oneLoginUser);

                var dateOfBirth = new DateOnly(1980, 3, 31);

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.DateOfBirth(coordinator.InstanceId))
                {
                    Content = new FormUrlEncodedContent(new Dictionary<string, string?>
                    {
                        { "DateOfBirth.Day", "31" },
                        { "DateOfBirth.Month", "3" },
                        { "DateOfBirth.Year", "1980" }
                    })
                };

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(JourneyUrls.NationalInsuranceNumber(coordinator.InstanceId), response.Headers.Location?.ToString());

                var state = coordinator.State;
                Assert.Equal(dateOfBirth, state.DateOfBirth);
            });

    private async Task SetupInstanceStateAsync(SignInJourneyCoordinator coordinator, OneLoginUser oneLoginUser)
    {
        var ticket = CreateOneLoginAuthenticationTicket(vtr: AuthenticationOnly, oneLoginUser);
        await coordinator.OnOneLoginCallbackAsync(ticket);
        coordinator.OnVerificationFailed();
        AddUrlToPath(coordinator, StepUrls.Name);
        AddUrlToPath(coordinator, StepUrls.DateOfBirth);
    }
}
