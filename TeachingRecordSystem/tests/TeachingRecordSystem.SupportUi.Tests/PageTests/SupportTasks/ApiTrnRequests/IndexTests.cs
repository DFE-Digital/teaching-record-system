using System.Diagnostics;
using AngleSharp.Html.Dom;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.ApiTrnRequests;

[Collection(nameof(DisableParallelization))]
public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture), IAsyncLifetime
{
    [Fact]
    public async Task Get_NoOpenTasks_ShowsNoTasksMessage()
    {
        // Arrange
        // TODO Create a Closed task when we can set them up

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/api-trn-requests/");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.NotNull(doc.GetElementByTestId("no-tasks-message"));
    }

    [Fact]
    public async Task Get_WithTask_ShowsExpectedDataInResultsTable()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/api-trn-requests/");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var resultRow = doc.GetElementByTestId("results")
            ?.GetElementsByTagName("tbody")
            .FirstOrDefault()
            ?.GetElementsByTagName("tr")
            .FirstOrDefault();

        Assert.NotNull(resultRow);
        AssertRowHasContent("name", $"{supportTask.TrnRequestMetadata!.FirstName} {supportTask.TrnRequestMetadata!.LastName}");
        AssertRowHasContent("email", supportTask.TrnRequestMetadata!.EmailAddress ?? string.Empty);
        AssertRowHasContent("requested-on", supportTask.CreatedOn.ToString(UiDefaults.DateOnlyDisplayFormat));
        AssertRowHasContent("source", applicationUser.Name);

        void AssertRowHasContent(string testId, string expectedText)
        {
            var column = resultRow.GetElementByTestId(testId);
            Assert.NotNull(column);
            Assert.Equal(expectedText, column.TextContent);
        }
    }

    [Fact]
    public async Task Get_SearchByFirstName_ShowsMatchingResult()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var firstName = TestData.GenerateFirstName();
        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            configure: t => t.WithFirstName(firstName));

        var search = firstName;

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/api-trn-requests/?Search={Uri.EscapeDataString(search)}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        doc.AssertResultsContainsTask(supportTask.SupportTaskReference);
    }

    [Fact]
    public async Task Get_SearchByMiddleName_ShowsMatchingResult()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var middleName = TestData.GenerateMiddleName();
        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            configure: t => t.WithMiddleName(middleName));

        var search = middleName;

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/api-trn-requests/?Search={Uri.EscapeDataString(search)}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        doc.AssertResultsContainsTask(supportTask.SupportTaskReference);
    }

    [Fact]
    public async Task Get_SearchByLastName_ShowsMatchingResult()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var lastName = TestData.GenerateLastName();
        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            configure: t => t.WithLastName(lastName));

        var search = lastName;

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/api-trn-requests/?Search={Uri.EscapeDataString(search)}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        doc.AssertResultsContainsTask(supportTask.SupportTaskReference);
    }

    [Fact]
    public async Task Get_SearchByMultipleNameParts_ShowsMatchingResult()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var firstName = TestData.GenerateFirstName();
        var lastName = TestData.GenerateFirstName();
        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            configure: t => t.WithFirstName(firstName).WithLastName(lastName));

        var search = $"{firstName} {lastName}";

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/api-trn-requests/?Search={Uri.EscapeDataString(search)}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        doc.AssertResultsContainsTask(supportTask.SupportTaskReference);
    }

    [Fact]
    public async Task Get_SearchByEmailAddress_ShowsMatchingResult()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var emailAddress = TestData.GenerateUniqueEmail();
        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            configure: t => t.WithEmailAddress(emailAddress));

        var search = emailAddress;

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/api-trn-requests/?Search={Uri.EscapeDataString(search)}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        doc.AssertResultsContainsTask(supportTask.SupportTaskReference);
    }

    [Theory]
    [InlineData("d/M/yyyy")]
    [InlineData("dd/MM/yyyy")]
    [InlineData(UiDefaults.DateOnlyDisplayFormat)]
    public async Task Get_SearchByRequestDate_ShowsMatchingResult(string dateFormat)
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var search = supportTask.CreatedOn.ToString(dateFormat);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/api-trn-requests/?Search={Uri.EscapeDataString(search)}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        doc.AssertResultsContainsTask(supportTask.SupportTaskReference);
    }

    public Task InitializeAsync() => WithDbContext(dbContext =>
        dbContext.SupportTasks.Where(t => t.SupportTaskType == SupportTaskType.ApiTrnRequest).ExecuteDeleteAsync());

    public Task DisposeAsync() => Task.CompletedTask;
}

file static class Extensions
{
    public static void AssertResultsContainsTask(this IHtmlDocument document, string supportTaskReference) =>
        Assert.NotNull(document.GetElementByTestId($"task:{supportTaskReference}"));
}
