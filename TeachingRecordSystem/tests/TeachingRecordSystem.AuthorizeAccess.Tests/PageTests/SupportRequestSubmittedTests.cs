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
                var doc = await AssertEx.HtmlResponseAsync(response);
                Assert.Null(doc.QuerySelector("form button[type='submit']"));
            });

    [Fact]
    public async Task Get_WithDeferredRecordMatchingAndAuthenticationTicket_ShowsReturnToServiceButton()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync(isOidcClient: true, recordMatchingPolicy: RecordMatchingPolicy.Deferred);

        await WithJourneyCoordinatorAsync(
            (instanceId, processId) => CreateSignInJourneyState(instanceId, processId, "/", applicationUser.UserId, null, null, RecordMatchingPolicy.Deferred),
            async coordinator =>
            {
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                await SetupInstanceStateWithAuthenticationTicketAsync(coordinator, oneLoginUser, applicationUser);

                var request = new HttpRequestMessage(HttpMethod.Get, JourneyUrls.RequestSubmitted(coordinator.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                var doc = await AssertEx.HtmlResponseAsync(response);

                var returnButton = doc.QuerySelector("form button[type='submit']");
                Assert.NotNull(returnButton);
                Assert.Contains("Test Service", returnButton.TextContent);
            });
    }

    [Fact]
    public async Task Post_WithAuthenticationTicket_RedirectsToCallbackUri()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync(isOidcClient: true, recordMatchingPolicy: RecordMatchingPolicy.Deferred);

        await WithJourneyCoordinatorAsync(
            (instanceId, processId) => CreateSignInJourneyState(instanceId, processId, "/", applicationUser.UserId, null, null, RecordMatchingPolicy.Deferred),
            async coordinator =>
            {
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                await SetupInstanceStateWithAuthenticationTicketAsync(coordinator, oneLoginUser, applicationUser);

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.RequestSubmitted(coordinator.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.NotNull(response.Headers.Location);
                Assert.Equal(coordinator.State.RedirectUri, response.Headers.Location.OriginalString);
            });
    }

    private async Task SetupInstanceStateWithAuthenticationTicketAsync(
        SignInJourneyCoordinator coordinator,
        OneLoginUser oneLoginUser,
        ApplicationUser applicationUser)
    {
        var ticket = CreateOneLoginAuthenticationTicket(vtr: AuthenticationOnly, oneLoginUser);
        await coordinator.OnOneLoginCallbackAsync(ticket);

        // Create a dormant TRN request and set up state with authentication ticket
        var trnRequestId = Guid.NewGuid().ToString();

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.TrnRequestMetadata.Add(new TrnRequestMetadata
            {
                ApplicationUserId = applicationUser.UserId,
                RequestId = trnRequestId,
                CreatedOn = Clock.UtcNow,
                OneLoginUserSubject = oneLoginUser.Subject,
                IdentityVerified = true,
                EmailAddress = oneLoginUser.EmailAddress,
                FirstName = "Test",
                MiddleName = null,
                LastName = "User",
                Name = ["Test", "User"],
                DateOfBirth = new DateOnly(1990, 1, 1)
            });
            await dbContext.SaveChangesAsync();
        });

        await coordinator.UpdateStateAsync(async state =>
        {
            await coordinator.CompleteWithDeferredMatchingAsync(state);
            state.CreatedSupportTaskReference = "TEST-REF-123";
            return state;
        });

        coordinator.UnsafeSetPath(new JourneyPath([coordinator.CreateStepFromUrl(StepUrls.RequestSubmitted)]));
    }

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
