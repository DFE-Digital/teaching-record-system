using TeachingRecordSystem.Core.DataStore.Postgres.Models;

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

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Get_WithReturnUrl_JourneyCompletesToReturnUrl(bool isRecordMatchingOnlySupportTask)
    {
        // Arrange
        var returnUrl = "/support-tasks/active?keyword=test";
        var (_, firstStepUrl) = await StartJourneyAsync(returnUrl, isRecordMatchingOnlySupportTask);

        var request = new HttpRequestMessage(HttpMethod.Post, firstStepUrl)
        {
            Content = new FormUrlEncodedContentBuilder { { "cancel", "true" } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(returnUrl, response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Get_WithNonLocalReturnUrl_JourneyCompletesToListPage(bool isRecordMatchingOnlySupportTask)
    {
        // Arrange
        var (supportTask, firstStepUrl) = await StartJourneyAsync("https://evil.example.com/", isRecordMatchingOnlySupportTask);

        var request = new HttpRequestMessage(HttpMethod.Post, firstStepUrl)
        {
            Content = new FormUrlEncodedContentBuilder { { "cancel", "true" } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(GetDefaultCompletionUrl(supportTask), response.Headers.Location?.OriginalString);
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
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, verifiedInfo: ([person.FirstName, person.LastName], person.DateOfBirth!.Value));
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

    /// <summary>
    /// Starts the journey the way the "View task" link does and returns the URL of its first page.
    /// </summary>
    private async Task<(SupportTask SupportTask, string FirstStepUrl)> StartJourneyAsync(
        string returnUrl,
        bool isRecordMatchingOnlySupportTask = true)
    {
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: isRecordMatchingOnlySupportTask);
        var supportTask = isRecordMatchingOnlySupportTask ?
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject) :
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve?returnUrl={Uri.EscapeDataString(returnUrl)}");

        var response = await HttpClient.SendAsync(request);  // Initializes journey
        response = await response.FollowRedirectAsync(HttpClient);

        return (supportTask, response.Headers.Location!.OriginalString);
    }
}
