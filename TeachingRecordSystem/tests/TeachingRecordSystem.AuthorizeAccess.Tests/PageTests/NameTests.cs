using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using static TeachingRecordSystem.AuthorizeAccess.SignInJourneyCoordinator.Vtrs;

namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests;

public class NameTests(HostFixture hostFixture) : TestBase(hostFixture)
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

                var existingFirstName = haveExistingValueInState ? TestData.GenerateFirstName() : null;
                var existingLastName = haveExistingValueInState ? TestData.GenerateLastName() : null;

                if (haveExistingValueInState)
                {
                    coordinator.UpdateState(state => state.SetName(existingFirstName!, existingLastName!));
                }

                var request = new HttpRequestMessage(HttpMethod.Get, JourneyUrls.Name(coordinator.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                var doc = await AssertEx.HtmlResponseAsync(response);
                Assert.Equal(existingFirstName, doc.GetElementById("FirstName")?.GetAttribute("value"));
                Assert.Equal(existingLastName, doc.GetElementById("LastName")?.GetAttribute("value"));
            });

    [Theory]
    [InlineData("", "Smith", "FirstName", "Enter your first name")]
    [InlineData("John", "", "LastName", "Enter your last name")]
    public Task Post_InvalidRequest_ShowsExpectedError(string firstName, string lastName, string expectedErrorFieldName, string expectedErrorMessage) =>
        WithJourneyCoordinatorAsync(
            CreateSignInJourneyState,
            async coordinator =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);

                await SetupInstanceStateAsync(coordinator, oneLoginUser);

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.Name(coordinator.InstanceId))
                {
                    Content = new FormUrlEncodedContent(new Dictionary<string, string?>
                    {
                        { "FirstName", firstName },
                        { "LastName", lastName }
                    })
                };

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                await AssertEx.HtmlResponseHasErrorAsync(response, expectedErrorFieldName, expectedErrorMessage);
            });

    [Fact]
    public Task Post_ValidRequest_UpdatesStateAndRedirectsToDateOfBirthPage() =>
        WithJourneyCoordinatorAsync(
            CreateSignInJourneyState,
            async coordinator =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);

                await SetupInstanceStateAsync(coordinator, oneLoginUser);

                var firstName = TestData.GenerateFirstName();
                var lastName = TestData.GenerateLastName();

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.Name(coordinator.InstanceId))
                {
                    Content = new FormUrlEncodedContent(new Dictionary<string, string?>
                    {
                        { "FirstName", firstName },
                        { "LastName", lastName }
                    })
                };

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(JourneyUrls.DateOfBirth(coordinator.InstanceId), response.Headers.Location?.ToString());

                var state = coordinator.State;
                Assert.Equal(firstName, state.FirstName);
                Assert.Equal(lastName, state.LastName);
            });

    private async Task SetupInstanceStateAsync(SignInJourneyCoordinator coordinator, OneLoginUser oneLoginUser)
    {
        var ticket = CreateOneLoginAuthenticationTicket(vtr: AuthenticationOnly, oneLoginUser);
        await coordinator.OnOneLoginCallbackAsync(ticket);
        coordinator.OnVerificationFailed();
        AddUrlToPath(coordinator, StepUrls.Name);
    }
}
