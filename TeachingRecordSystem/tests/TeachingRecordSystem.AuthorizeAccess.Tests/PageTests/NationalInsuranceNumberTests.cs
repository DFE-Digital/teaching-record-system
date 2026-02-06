using System.Diagnostics;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using static TeachingRecordSystem.AuthorizeAccess.SignInJourneyCoordinator.Vtrs;

namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests;

public class NationalInsuranceNumberTests(HostFixture hostFixture) : TestBase(hostFixture)
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

                await SetupInstanceStateForVerifiedUserAsync(coordinator, oneLoginUser);

                var existingNationalInsuranceNumber = haveExistingValueInState ? Faker.Identification.UkNationalInsuranceNumber() : null;
                if (existingNationalInsuranceNumber is not null)
                {
                    coordinator.UpdateState(s => s.SetNationalInsuranceNumber(true, existingNationalInsuranceNumber));
                }

                var request = new HttpRequestMessage(HttpMethod.Get, JourneyUrls.NationalInsuranceNumber(coordinator.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                var doc = await AssertEx.HtmlResponseAsync(response);
                Assert.Equal(existingNationalInsuranceNumber, doc.GetElementById("NationalInsuranceNumber")?.GetAttribute("value"));
            });

    [Fact]
    public Task Post_HaveNationalInsuranceNumberNotAnswered_RendersError() =>
        WithJourneyCoordinatorAsync(
            CreateSignInJourneyState,
            async coordinator =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                await SetupInstanceStateForVerifiedUserAsync(coordinator, oneLoginUser);

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.NationalInsuranceNumber(coordinator.InstanceId))
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
        WithJourneyCoordinatorAsync(
            CreateSignInJourneyState,
            async coordinator =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                await SetupInstanceStateForVerifiedUserAsync(coordinator, oneLoginUser);

                var nationalInsuranceNumber = "";

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.NationalInsuranceNumber(coordinator.InstanceId))
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
        WithJourneyCoordinatorAsync(
            CreateSignInJourneyState,
            async coordinator =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                await SetupInstanceStateForVerifiedUserAsync(coordinator, oneLoginUser);

                var nationalInsuranceNumber = "xxx";

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.NationalInsuranceNumber(coordinator.InstanceId))
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
    public Task Post_HaveNationalInsuranceNumberNotSpecifiedForVerifiedUser_UpdatesStateAndRedirectsToTrnPage() =>
        WithJourneyCoordinatorAsync(
            CreateSignInJourneyState,
            async coordinator =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                await SetupInstanceStateForVerifiedUserAsync(coordinator, oneLoginUser);

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.NationalInsuranceNumber(coordinator.InstanceId))
                {
                    Content = new FormUrlEncodedContentBuilder { { "HaveNationalInsuranceNumber", bool.FalseString } }
                };

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(JourneyUrls.Trn(coordinator.InstanceId), response.Headers.Location?.OriginalString);

                var state = coordinator.State;
                Assert.False(state.HaveNationalInsuranceNumber);
                Assert.Null(state.AuthenticationTicket);
            });

    [Fact]
    public Task Post_ValidNationalInsuranceNumberButLookupFailedForVerifiedUser_UpdatesStateAndRedirectsToTrnPage() =>
        WithJourneyCoordinatorAsync(
            CreateSignInJourneyState,
            async coordinator =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                await SetupInstanceStateForVerifiedUserAsync(coordinator, oneLoginUser);

                var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.NationalInsuranceNumber(coordinator.InstanceId))
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
                Assert.Equal(JourneyUrls.Trn(coordinator.InstanceId), response.Headers.Location?.OriginalString);

                var state = coordinator.State;
                Assert.True(state.HaveNationalInsuranceNumber);
                Assert.Equal(nationalInsuranceNumber, state.NationalInsuranceNumber);
                Assert.Null(state.AuthenticationTicket);
            });

    [Fact]
    public Task Post_ValidNationalInsuranceNumberAndLookupSucceededForVerifiedUser_UpdatesStateUpdatesOneLoginUserCompletesAuthenticationAndRedirectsToFoundPage() =>
        WithJourneyCoordinatorAsync(
            CreateSignInJourneyState,
            async coordinator =>
            {
                // Arrange
                var person = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber());
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(
                    personId: null,
                    verifiedInfo: ([person.FirstName, person.LastName], person.DateOfBirth));

                await SetupInstanceStateForVerifiedUserAsync(coordinator, oneLoginUser);

                var nationalInsuranceNumber = person.NationalInsuranceNumber!;

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.NationalInsuranceNumber(coordinator.InstanceId))
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
                Assert.Equal(JourneyUrls.Found(coordinator.InstanceId), response.Headers.Location?.OriginalString);

                var state = coordinator.State;
                Assert.True(state.HaveNationalInsuranceNumber);
                Assert.Equal(nationalInsuranceNumber, state.NationalInsuranceNumber);
                Assert.NotNull(state.AuthenticationTicket);

                oneLoginUser = await WithDbContextAsync(dbContext => dbContext.OneLoginUsers.SingleAsync(u => u.Subject == oneLoginUser.Subject));
                Assert.Equal(person.PersonId, oneLoginUser.PersonId);
            });

    [Fact]
    public Task Post_HaveNationalInsuranceNumberNotSpecifiedForUnverifiedUser_UpdatesStateAndRedirectsToTrnPage() =>
        WithJourneyCoordinatorAsync(
            CreateSignInJourneyState,
            async coordinator =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);

                await SetupInstanceForUnverifiedUserStateAsync(coordinator, oneLoginUser);

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.NationalInsuranceNumber(coordinator.InstanceId))
                {
                    Content = new FormUrlEncodedContentBuilder { { "HaveNationalInsuranceNumber", bool.FalseString } }
                };

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(JourneyUrls.Trn(coordinator.InstanceId), response.Headers.Location?.OriginalString);

                var state = coordinator.State;
                Assert.False(state.HaveNationalInsuranceNumber);
                Assert.Null(state.AuthenticationTicket);
            });

    [Fact]
    public Task Post_ValidNationalInsuranceNumberForVerifiedUser_UpdatesStateAndRedirectsToTrnPage() =>
        WithJourneyCoordinatorAsync(
            CreateSignInJourneyState,
            async coordinator =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);

                await SetupInstanceForUnverifiedUserStateAsync(coordinator, oneLoginUser);

                var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.NationalInsuranceNumber(coordinator.InstanceId))
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
                Assert.Equal(JourneyUrls.Trn(coordinator.InstanceId), response.Headers.Location?.OriginalString);

                var state = coordinator.State;
                Assert.True(state.HaveNationalInsuranceNumber);
                Assert.Equal(nationalInsuranceNumber, state.NationalInsuranceNumber);
                Assert.Null(state.AuthenticationTicket);
            });

    private async Task SetupInstanceStateForVerifiedUserAsync(SignInJourneyCoordinator coordinator, OneLoginUser oneLoginUser)
    {
        var ticket = CreateOneLoginAuthenticationTicket(vtr: AuthenticationOnly, oneLoginUser);
        await coordinator.OnOneLoginCallbackAsync(ticket);
        Debug.Assert(coordinator.State.IdentityVerified);
        AddUrlToPath(coordinator, StepUrls.NationalInsuranceNumber);
    }

    private async Task SetupInstanceForUnverifiedUserStateAsync(
        SignInJourneyCoordinator coordinator,
        OneLoginUser oneLoginUser)
    {
        var ticket = CreateOneLoginAuthenticationTicket(vtr: AuthenticationOnly, oneLoginUser);
        await coordinator.OnOneLoginCallbackAsync(ticket);
        Debug.Assert(!coordinator.State.IdentityVerified);
        AddUrlToPath(coordinator, StepUrls.Name);
        coordinator.UpdateState(s => s.SetName(TestData.GenerateFirstName(), TestData.GenerateLastName()));
        AddUrlToPath(coordinator, StepUrls.DateOfBirth);
        coordinator.UpdateState(s => s.SetDateOfBirth(TestData.GenerateDateOfBirth()));
        AddUrlToPath(coordinator, StepUrls.NationalInsuranceNumber);
    }
}
