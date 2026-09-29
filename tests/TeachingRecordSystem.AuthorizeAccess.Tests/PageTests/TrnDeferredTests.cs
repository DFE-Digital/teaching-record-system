using System.Diagnostics;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using static TeachingRecordSystem.AuthorizeAccess.SignInJourneyCoordinator.Vtrs;

namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests;

public class TrnDeferredTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Post_ValidRequest_CreatesDormantTrnRequestAndCompletesSignIn()
    {
        // Arrange
        var clientApplicationUser = await TestData.CreateApplicationUserAsync(recordMatchingPolicy: RecordMatchingPolicy.Deferred);

        await WithJourneyCoordinatorAsync(
            (instanceId, processId) => CreateSignInJourneyState(instanceId, processId, "/", clientApplicationUser.UserId, recordMatchingPolicy: RecordMatchingPolicy.Deferred),
            async coordinator =>
            {
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                await SetupInstanceStateAsync(coordinator, oneLoginUser);

                var redirectUri = coordinator.State.RedirectUri;
                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.TrnDeferred(coordinator.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(redirectUri, response.Headers.Location?.OriginalString);

                Assert.NotNull(coordinator.State.AuthenticationTicket);

                var trnRequestCount = await WithDbContextAsync(dbContext =>
                    dbContext.TrnRequestMetadata.CountAsync(r => r.ApplicationUserId == clientApplicationUser.UserId));
                Assert.Equal(1, trnRequestCount);
            });
    }

    [Fact]
    public async Task Post_UserConnectedInAnotherJourney_DoesNotCreateTrnRequestAndCompletesSignIn()
    {
        // Arrange
        var clientApplicationUser = await TestData.CreateApplicationUserAsync(recordMatchingPolicy: RecordMatchingPolicy.Deferred);
        var connectedPerson = await TestData.CreatePersonAsync();

        await WithJourneyCoordinatorAsync(
            (instanceId, processId) => CreateSignInJourneyState(instanceId, processId, "/", clientApplicationUser.UserId, recordMatchingPolicy: RecordMatchingPolicy.Deferred),
            async coordinator =>
            {
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                await SetupInstanceStateAsync(coordinator, oneLoginUser);

                await SetOneLoginUserConnectedAsync(oneLoginUser.Subject, connectedPerson.PersonId);

                var redirectUri = coordinator.State.RedirectUri;
                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.TrnDeferred(coordinator.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(redirectUri, response.Headers.Location?.OriginalString);

                Assert.NotNull(coordinator.State.AuthenticationTicket);

                var trnRequestCount = await WithDbContextAsync(dbContext =>
                    dbContext.TrnRequestMetadata.CountAsync(r => r.ApplicationUserId == clientApplicationUser.UserId));
                Assert.Equal(0, trnRequestCount);
            });
    }

    private async Task SetupInstanceStateAsync(SignInJourneyCoordinator coordinator, OneLoginUser oneLoginUser)
    {
        var ticket = CreateOneLoginAuthenticationTicket(vtr: AuthenticationOnly, oneLoginUser);
        await coordinator.OnOneLoginCallbackAsync(ticket);
        Debug.Assert(coordinator.State.IdentityVerified);
        AddUrlToPath(coordinator, StepUrls.NationalInsuranceNumber);
        coordinator.UpdateState(s => s.SetNationalInsuranceNumber(true, TestData.GenerateNationalInsuranceNumber()));
        AddUrlToPath(coordinator, StepUrls.Trn);
        coordinator.UpdateState(s => s.SetTrn(false, null));
        AddUrlToPath(coordinator, StepUrls.TrnDeferred);
    }
}
