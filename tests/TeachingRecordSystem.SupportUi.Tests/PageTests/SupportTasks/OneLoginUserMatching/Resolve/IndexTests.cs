namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.OneLoginUserMatching.Resolve;

[ClearDbBeforeTest, Collection(nameof(DisableParallelization))]
public class IndexTests(HostFixture hostFixture) : ResolveOneLoginUserMatchingTestBase(hostFixture)
{
    [Fact]
    public async Task Get_SupportTaskDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/one-login-user-matching/TRS-000/resolve");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_SupportTaskIsNotOpen_ReturnsNotFound()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        await WithDbContextAsync(dbContext => dbContext.SupportTasks
            .Where(t => t.SupportTaskReference == supportTask.SupportTaskReference)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.Status, _ => SupportTaskStatus.Closed)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Get_ValidRequest_RedirectsToExpectedStartPage(bool isRecordMatchingOnlySupportTask)
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: isRecordMatchingOnlySupportTask);
        var supportTask = isRecordMatchingOnlySupportTask ?
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject) :
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve");

        // Act
        var response = await HttpClient.SendAsync(request);  // Initializes journey
        response = await response.FollowRedirectAsync(HttpClient);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var expectedPage = isRecordMatchingOnlySupportTask ? "no-matches" : "verify";
        Assert.StartsWith(
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/{expectedPage}?",
            response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_SupportTaskDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/one-login-user-matching/TRS-000/resolve")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_SupportTaskIsNotOpen_ReturnsNotFound()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, verifiedInfo: ([person.FirstName, person.LastName], person.DateOfBirth));
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        await WithDbContextAsync(dbContext => dbContext.SupportTasks
            .Where(t => t.SupportTaskReference == supportTask.SupportTaskReference)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.Status, _ => SupportTaskStatus.Closed)));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }
}
