using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.ApiTrnRequests;

[NotInParallel]
public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Before(Test)]
    public Task DeleteApiTrnRequestSupportTasks() =>
        WithDbContext(dbContext =>
            dbContext.SupportTasks.Where(t => t.SupportTaskType == SupportTaskType.ApiTrnRequest).ExecuteDeleteAsync());

    [Test]
    public async Task Get_NoOpenTasks_ShowsNoTasksMessage()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId, t => t.WithStatus(SupportTaskStatus.Closed));

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/api-trn-requests/");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.NotNull(doc.GetElementByTestId("no-tasks-message"));
    }

    [Test]
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
        AssertRowHasContent("name", $"{supportTask.TrnRequestMetadata!.FirstName} {supportTask.TrnRequestMetadata!.MiddleName} {supportTask.TrnRequestMetadata!.LastName}");
        AssertRowHasContent("email", supportTask.TrnRequestMetadata!.EmailAddress ?? string.Empty);
        AssertRowHasContent("requested-on", supportTask.CreatedOn.ToString(UiDefaults.DateOnlyDisplayFormat));
        AssertRowHasContent("source", applicationUser.Name);

        void AssertRowHasContent(string testId, string expectedText)
        {
            var column = resultRow.GetElementByTestId(testId);
            Assert.NotNull(column);
            Assert.Equal(expectedText, column.TrimmedText());
        }
    }

    [Test]
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

    [Test]
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

    [Test]
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

    [Test]
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

    [Test]
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

    [Test]
    [Arguments("d/M/yyyy")]
    [Arguments("dd/MM/yyyy")]
    [Arguments(UiDefaults.DateOnlyDisplayFormat)]
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

    [Test]
    public async Task Get_NoSortParametersSpecified_ShowsRequestsOrderedByRequestedOnAscending()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask1 = await TestData.CreateApiTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t.WithCreatedOn(new DateTime(2025, 1, 1)));

        var supportTask2 = await TestData.CreateApiTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t.WithCreatedOn(new DateTime(2023, 10, 10)));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Collection(
            GetResultTaskReferences(doc),
            result => Assert.Equal(supportTask2.SupportTaskReference, result),
            result => Assert.Equal(supportTask1.SupportTaskReference, result));
    }

    [Test]
    public async Task Get_SortByNameAscending_ShowsRequestsInCorrectOrder()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask1 = await TestData.CreateApiTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t.WithFirstName("Zavier"));

        var supportTask2 = await TestData.CreateApiTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t.WithFirstName("Aaron"));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/?sortBy={ApiTrnRequestsSortByOption.Name}&sortDirection={SortDirection.Ascending}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Collection(
            GetResultTaskReferences(doc),
            result => Assert.Equal(supportTask2.SupportTaskReference, result),
            result => Assert.Equal(supportTask1.SupportTaskReference, result));
    }

    [Test]
    public async Task Get_SortByNameDescending_ShowsRequestsInCorrectOrder()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask1 = await TestData.CreateApiTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t.WithFirstName("Zavier"));

        var supportTask2 = await TestData.CreateApiTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t.WithFirstName("Aaron"));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/?sortBy={ApiTrnRequestsSortByOption.Name}&sortDirection={SortDirection.Descending}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Collection(
            GetResultTaskReferences(doc),
            result => Assert.Equal(supportTask1.SupportTaskReference, result),
            result => Assert.Equal(supportTask2.SupportTaskReference, result));
    }

    [Test]
    public async Task Get_SortByEmailAscending_ShowsRequestsInCorrectOrder()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask1 = await TestData.CreateApiTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t.WithEmailAddress("zavier@example.com"));

        var supportTask2 = await TestData.CreateApiTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t.WithEmailAddress("aaron@example.com"));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/?sortBy={ApiTrnRequestsSortByOption.Email}&sortDirection={SortDirection.Ascending}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Collection(
            GetResultTaskReferences(doc),
            result => Assert.Equal(supportTask2.SupportTaskReference, result),
            result => Assert.Equal(supportTask1.SupportTaskReference, result));
    }

    [Test]
    public async Task Get_SortByEmailDescending_ShowsRequestsInCorrectOrder()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask1 = await TestData.CreateApiTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t.WithEmailAddress("zavier@example.com"));

        var supportTask2 = await TestData.CreateApiTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t.WithEmailAddress("aaron@example.com"));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/?sortBy={ApiTrnRequestsSortByOption.Email}&sortDirection={SortDirection.Descending}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Collection(
            GetResultTaskReferences(doc),
            result => Assert.Equal(supportTask1.SupportTaskReference, result),
            result => Assert.Equal(supportTask2.SupportTaskReference, result));
    }

    [Test]
    public async Task Get_SortByRequestedOnAscending_ShowsRequestsInCorrectOrder()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask1 = await TestData.CreateApiTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t.WithCreatedOn(new DateTime(2025, 1, 1)));

        var supportTask2 = await TestData.CreateApiTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t.WithCreatedOn(new DateTime(2023, 10, 10)));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/?sortBy={ApiTrnRequestsSortByOption.RequestedOn}&sortDirection={SortDirection.Ascending}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Collection(
            GetResultTaskReferences(doc),
            result => Assert.Equal(supportTask2.SupportTaskReference, result),
            result => Assert.Equal(supportTask1.SupportTaskReference, result));
    }

    [Test]
    public async Task Get_SortByRequestedOnDescending_ShowsRequestsInCorrectOrder()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask1 = await TestData.CreateApiTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t.WithCreatedOn(new DateTime(2025, 1, 1)));

        var supportTask2 = await TestData.CreateApiTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t.WithCreatedOn(new DateTime(2023, 10, 10)));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/?sortBy={ApiTrnRequestsSortByOption.RequestedOn}&sortDirection={SortDirection.Descending}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Collection(
            GetResultTaskReferences(doc),
            result => Assert.Equal(supportTask1.SupportTaskReference, result),
            result => Assert.Equal(supportTask2.SupportTaskReference, result));
    }

    [Test]
    public async Task Get_SortBySourceAscending_ShowsRequestsInCorrectOrder()
    {
        // Arrange
        var applicationUser1 = await TestData.CreateApplicationUserAsync(name: "Application Z");
        var applicationUser2 = await TestData.CreateApplicationUserAsync(name: "Application A");

        var supportTask1 = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser1.UserId);

        var supportTask2 = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser2.UserId);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/?sortBy={ApiTrnRequestsSortByOption.Source}&sortDirection={SortDirection.Ascending}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Collection(
            GetResultTaskReferences(doc),
            result => Assert.Equal(supportTask2.SupportTaskReference, result),
            result => Assert.Equal(supportTask1.SupportTaskReference, result));
    }

    [Test]
    public async Task Get_SortBySourceDescending_ShowsRequestsInCorrectOrder()
    {
        // Arrange
        var applicationUser1 = await TestData.CreateApplicationUserAsync(name: "Application Z");
        var applicationUser2 = await TestData.CreateApplicationUserAsync(name: "Application A");

        var supportTask1 = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser1.UserId);

        var supportTask2 = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser2.UserId);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/?sortBy={ApiTrnRequestsSortByOption.Source}&sortDirection={SortDirection.Descending}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Collection(
            GetResultTaskReferences(doc),
            result => Assert.Equal(supportTask1.SupportTaskReference, result),
            result => Assert.Equal(supportTask2.SupportTaskReference, result));
    }

    [Test]
    public async Task Get_ShowsPageOfResults()
    {
        // Arrange
        var pageSize = 20;
        var page = 2;

        var applicationUser = await TestData.CreateApplicationUserAsync();

        // Create enough tasks to create 3 pages
        var tasks = await Enumerable.Range(1, (pageSize * page) + 1)
            .ToAsyncEnumerable()
            .SelectAwait(async _ => await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId))
            .ToArrayAsync();

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/?pageNumber={page}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(pageSize, GetResultTaskReferences(doc).Length);
    }

    [Test]
    public async Task Get_UsesApplicationShortNameIfSetOtherwiseApplicationName()
    {
        // Arrange
        var applicationUserWithShortName = await TestData.CreateApplicationUserAsync(shortName: TestData.GenerateApplicationUserShortName());
        var applicationUserWithoutShortName = await TestData.CreateApplicationUserAsync(shortName: "");

        await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUserWithShortName.UserId);
        Clock.Advance();
        await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUserWithoutShortName.UserId);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/?sortBy={ApiTrnRequestsSortByOption.RequestedOn}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var resultRows = GetResultRows(doc);
        Assert.Collection(
            resultRows,
            row => AssertSourceColumnEquals(row, applicationUserWithShortName.ShortName!),
            row => AssertSourceColumnEquals(row, applicationUserWithoutShortName.Name));

        static void AssertSourceColumnEquals(IElement row, string expectedSource)
        {
            var source = row.Children[3];
            Assert.Equal(expectedSource, source.TrimmedText());
        }
    }

    private static IElement[] GetResultRows(IHtmlDocument doc) =>
        doc
            .GetElementsByTagName("tbody")
            .Single()
            .GetElementsByClassName("govuk-table__row")
            .ToArray();

    private static string[] GetResultTaskReferences(IHtmlDocument doc) =>
        GetResultRows(doc)
            .Select(row => row.GetAttribute("data-testid")!["task:".Length..])
            .ToArray();
}

file static class Extensions
{
    public static void AssertResultsContainsTask(this IHtmlDocument document, string supportTaskReference) =>
        Assert.NotNull(document.GetElementByTestId($"task:{supportTaskReference}"));
}
