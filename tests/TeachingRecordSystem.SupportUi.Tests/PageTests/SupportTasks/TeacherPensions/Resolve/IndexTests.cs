using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.TeacherPensions.Resolve;

public class IndexTests(HostFixture hostFixture) : ResolveTeacherPensionsPotentialDuplicateTestBase(hostFixture)
{
    [Fact]
    public async Task Get_PotentialDuplicateTaskDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/teacher-pensions/TRS-000/resolve");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequest_StartsJourneyAndRedirectsToMatches()
    {
        // Arrange
        var supportTask = await CreateSupportTaskAsync();

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/teacher-pensions/{supportTask.SupportTaskReference}/resolve");

        // Act
        var response = await HttpClient.SendAsync(request);  // Initializes journey
        response = await response.FollowRedirectAsync(HttpClient);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith(
            $"/support-tasks/teacher-pensions/{supportTask.SupportTaskReference}/resolve/matches?",
            response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_ViaSupportTaskResolveLink_StartsJourney()
    {
        // The "View task" link on the support task pages used to point straight at the Matches page,
        // which has no journey instance to work with.
        // Arrange
        var supportTask = await CreateSupportTaskAsync();

        var linkGenerator = HostFixture.Services.GetRequiredService<SupportUiLinkGenerator>();
        var resolveLink = linkGenerator.SupportTaskResolve(
            supportTask.SupportTaskReference,
            SupportTaskType.TeacherPensionsPotentialDuplicate);

        // Act
        var response = await HttpClient.GetAsync(resolveLink);
        response = await response.FollowRedirectAsync(HttpClient);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith(
            $"/support-tasks/teacher-pensions/{supportTask.SupportTaskReference}/resolve/matches?",
            response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_WithReturnUrl_JourneyCompletesToReturnUrl()
    {
        // Arrange
        var returnUrl = "/support-tasks/active?keyword=test";
        var matchesUrl = await StartJourneyAsync(returnUrl);

        var request = new HttpRequestMessage(HttpMethod.Post, matchesUrl)
        {
            Content = new FormUrlEncodedContentBuilder { { "Cancel", "True" } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(returnUrl, response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_WithNonLocalReturnUrl_JourneyCompletesToListPage()
    {
        // Arrange
        var matchesUrl = await StartJourneyAsync("https://evil.example.com/");

        var request = new HttpRequestMessage(HttpMethod.Post, matchesUrl)
        {
            Content = new FormUrlEncodedContentBuilder { { "Cancel", "True" } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(DefaultCompletionUrl, response.Headers.Location?.OriginalString);
    }

    /// <summary>
    /// Starts the journey the way the "View task" link does and returns the URL of its first page.
    /// </summary>
    private async Task<string> StartJourneyAsync(string returnUrl)
    {
        var supportTask = await CreateSupportTaskAsync();

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/teacher-pensions/{supportTask.SupportTaskReference}/resolve?returnUrl={Uri.EscapeDataString(returnUrl)}");

        var response = await HttpClient.SendAsync(request);  // Initializes journey
        response = await response.FollowRedirectAsync(HttpClient);

        return response.Headers.Location!.OriginalString;
    }

    private async Task<SupportTask> CreateSupportTaskAsync()
    {
        var person = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber());
        var duplicatePerson = await TestData.CreatePersonAsync(x => x
            .WithFirstName(person.FirstName)
            .WithLastName(person.LastName)
            .WithNationalInsuranceNumber(person.NationalInsuranceNumber!));
        var user = await TestData.CreateUserAsync();

        return await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
            person.PersonId,
            user.UserId,
            s =>
            {
                s.WithMatchedPersons(duplicatePerson.PersonId);
                s.WithFirstName(person.FirstName);
                s.WithLastName(person.LastName);
                s.WithNationalInsuranceNumber(person.NationalInsuranceNumber);
                s.WithDateOfBirth(person.DateOfBirth!.Value);
                s.WithStatus(SupportTaskStatus.Open);
            });
    }
}
