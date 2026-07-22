namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.TrnRequests.Resolve;

public class IndexTests(HostFixture hostFixture) : ResolveApiTrnRequestTestBase(hostFixture)
{
    [Fact]
    public async Task Get_TaskDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/trn-requests/TRS-000/resolve");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequest_StartsJourneyAndRedirectsToMatches()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var (supportTask, _, _) = await TestData.CreateTrnRequestSupportTaskAsync(applicationUser.UserId);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/trn-requests/{supportTask.SupportTaskReference}/resolve");

        // Act
        var response = await HttpClient.SendAsync(request);  // Initializes journey
        response = await response.FollowRedirectAsync(HttpClient);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith(
            $"/support-tasks/trn-requests/{supportTask.SupportTaskReference}/resolve/matches?",
            response.Headers.Location?.OriginalString);
    }
}
