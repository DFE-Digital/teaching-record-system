using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.TrnRequestManualChecksNeeded.Resolve;

public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_TaskDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var supportTaskReference = SupportTask.GenerateSupportTaskReference();
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/manual-checks-needed/{supportTaskReference}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_TaskIsClosed_ReturnsNotFound()
    {
        // Arrange
        var supportTask = await CreateSupportTaskAsync(SupportTaskStatus.Closed);
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/manual-checks-needed/{supportTask.SupportTaskReference}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequest_ShowsRequestDetails()
    {
        // Arrange
        var supportTask = await CreateSupportTaskAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/manual-checks-needed/{supportTask.SupportTaskReference}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal("an alert, QTS and EYTS", doc.GetElementByTestId("flags-description")?.TextContent);
        var trnRequestMetadata = supportTask.TrnRequestMetadata!;
        Assert.Equal(trnRequestMetadata.FirstName, doc.GetSummaryListValueForKey("First name"));
        Assert.Equal(trnRequestMetadata.MiddleName, doc.GetSummaryListValueForKey("Middle name"));
        Assert.Equal(trnRequestMetadata.LastName, doc.GetSummaryListValueForKey("Last name"));
        Assert.Equal(trnRequestMetadata.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat), doc.GetSummaryListValueForKey("Date of birth"));
        Assert.Equal(trnRequestMetadata.EmailAddress, doc.GetSummaryListValueForKey("Email address"));
        Assert.Equal(trnRequestMetadata.NationalInsuranceNumber, doc.GetSummaryListValueForKey("National insurance number"));
        Assert.Collection(
            doc.GetSummaryListValueForKey("Flags")!.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            f => Assert.Equal("1 open alert", f),
            f => Assert.Equal("Holds QTS", f),
            f => Assert.Equal("Holds EYTS", f));
    }

    [Fact]
    public async Task Post_TaskDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var supportTaskReference = SupportTask.GenerateSupportTaskReference();
        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/manual-checks-needed/{supportTaskReference}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_TaskIsClosed_ReturnsNotFound()
    {
        // Arrange
        var supportTask = await CreateSupportTaskAsync(SupportTaskStatus.Closed);
        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/manual-checks-needed/{supportTask.SupportTaskReference}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_ChecksCompletedAnsweredYes_RedirectsToConfirmPage()
    {
        // Arrange
        var supportTask = await CreateSupportTaskAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/manual-checks-needed/{supportTask.SupportTaskReference}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "ChecksCompleted", "true" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/support-tasks/manual-checks-needed/{supportTask.SupportTaskReference}/confirm", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Post_ChecksCompletedNotAnswered_RendersError()
    {
        // Arrange
        var supportTask = await CreateSupportTaskAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/manual-checks-needed/{supportTask.SupportTaskReference}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "ChecksCompleted", "You must complete all checks before confirming");
    }

    [Fact]
    public async Task Post_ChecksCompletedAnsweredNo_RedirectsToListPage()
    {
        // Arrange
        var supportTask = await CreateSupportTaskAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/manual-checks-needed/{supportTask.SupportTaskReference}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "ChecksCompleted", "false" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal("/support-tasks/manual-checks-needed", response.Headers.Location?.ToString());
    }

    private async Task<SupportTask> CreateSupportTaskAsync(SupportTaskStatus status = SupportTaskStatus.Open)
    {
        var matchedPerson = await TestData.CreatePersonAsync(p => p.WithEmail(TestData.GenerateUniqueEmail()).WithAlert().WithQts().WithEyts());

        var applicationUser = await TestData.CreateApplicationUserAsync();

        var apiSupportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            matchedPerson.Person,
            t => t.WithTrnRequestStatus(TrnRequestStatus.Pending));

        return await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(
            apiSupportTask.TrnRequestMetadata!.ApplicationUserId,
            apiSupportTask.TrnRequestMetadata.RequestId,
            status);
    }
}
