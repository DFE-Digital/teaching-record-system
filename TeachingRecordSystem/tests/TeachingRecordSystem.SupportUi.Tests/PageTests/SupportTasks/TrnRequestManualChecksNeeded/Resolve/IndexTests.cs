using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.TrnRequestManualChecksNeeded.Resolve;

public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_TaskDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var supportTaskReference = "TASK-123";
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/manual-checks-needed/{supportTaskReference}/resolve");

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
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/manual-checks-needed/{supportTask.SupportTaskReference}/resolve");

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
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/manual-checks-needed/{supportTask.SupportTaskReference}/resolve");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal("an alert, QTS and EYTS", doc.GetElementByTestId("flags-description")?.TextContent);
        var trnRequestMetadata = supportTask.TrnRequestMetadata!;
        var cardActions = doc.QuerySelectorAll<IHtmlAnchorElement>(".govuk-summary-card__actions>*");
        Assert.Single(cardActions);
        Assert.Equal(trnRequestMetadata.FirstName, doc.GetSummaryListValueByKey("First name"));
        Assert.Equal(trnRequestMetadata.MiddleName, doc.GetSummaryListValueByKey("Middle name"));
        Assert.Equal(trnRequestMetadata.LastName, doc.GetSummaryListValueByKey("Last name"));
        Assert.Equal(trnRequestMetadata.DateOfBirth.ToString(WebConstants.DateOnlyDisplayFormat), doc.GetSummaryListValueByKey("Date of birth"));
        Assert.Equal(trnRequestMetadata.EmailAddress, doc.GetSummaryListValueByKey("Email address"));
        Assert.Equal(trnRequestMetadata.NationalInsuranceNumber, doc.GetSummaryListValueByKey("National insurance number"));
        Assert.Collection(
            doc.GetSummaryListValueByKey("Flags")!.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            f => Assert.Equal("1 open alert", f),
            f => Assert.Equal("Holds QTS", f),
            f => Assert.Equal("Holds EYTS", f));
    }

    [Fact]
    public async Task Post_TaskDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var supportTaskReference = "TASK-123";
        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/manual-checks-needed/{supportTaskReference}/resolve");

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
        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/manual-checks-needed/{supportTask.SupportTaskReference}/resolve");

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

        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/manual-checks-needed/{supportTask.SupportTaskReference}/resolve")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "ChecksCompleted", "true" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/support-tasks/manual-checks-needed/{supportTask.SupportTaskReference}/resolve/confirm", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Post_ChecksCompletedNotAnswered_RendersError()
    {
        // Arrange
        var supportTask = await CreateSupportTaskAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/manual-checks-needed/{supportTask.SupportTaskReference}/resolve")
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

        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/manual-checks-needed/{supportTask.SupportTaskReference}/resolve")
        {
            Content = new FormUrlEncodedContentBuilder
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
        var matchedPerson = await TestData.CreatePersonAsync(p => p.WithEmailAddress(TestData.GenerateUniqueEmail()).WithAlert().WithQts().WithEyts());

        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (apiSupportTask, _, _) = await TestData.CreateResolvedApiTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            matchedPerson.Person,
            t => t.WithTrnRequestStatus(TrnRequestStatus.Pending));

        return await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(
            apiSupportTask.TrnRequestMetadata!.ApplicationUserId,
            apiSupportTask.TrnRequestMetadata.RequestId,
            status);
    }
}
