using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.NpqTrnRequests;

[ClearDbBeforeTest, Collection(nameof(DisableParallelization))]
public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_NoOpenTasks_ShowsNoTasksMessage()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync(name: "NPQ");
        await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId, t => t.WithStatus(SupportTaskStatus.Closed));

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/npq-trn-requests/");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Empty(doc.GetElementsByTagName("table"));
        Assert.NotNull(doc.GetElementByTestId("no-tasks-message"));
        Assert.Null(doc.GetElementByTestId("no-results-message"));
    }

    [Fact]
    public async Task Get_WithTask_ButNotMatchingSearchCriteria_ShowsNoResultsMessage()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync(name: "NPQ");
        var firstName = TestData.GenerateFirstName();
        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            configure: t => t.WithFirstName(firstName));

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/npq-trn-requests/?Search=XXX");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Empty(doc.GetElementsByTagName("table"));
        Assert.Null(doc.GetElementByTestId("no-tasks-message"));
        Assert.NotNull(doc.GetElementByTestId("no-results-message"));
    }

    [Fact]
    public async Task Get_WithTask_ShowsExpectedDataInResultsTable()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync(name: "NPQ");
        var (supportTask, _, _) = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/npq-trn-requests/");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Null(doc.GetElementByTestId("no-tasks-message"));
        Assert.Null(doc.GetElementByTestId("no-results-message"));

        var resultRow = doc.GetElementByTestId("results")
            ?.GetElementsByTagName("tbody")
            .FirstOrDefault()
            ?.GetElementsByTagName("tr")
            .FirstOrDefault();

        Assert.NotNull(resultRow);
        AssertRowHasContent("name", $"{supportTask.TrnRequestMetadata!.FirstName} {supportTask.TrnRequestMetadata!.MiddleName} {supportTask.TrnRequestMetadata!.LastName}");
        AssertRowHasContent("email", supportTask.TrnRequestMetadata!.EmailAddress ?? string.Empty);
        AssertRowHasContent("requested-on", supportTask.CreatedOn.ToString(UiDefaults.DateTimeDisplayFormat));
        AssertRowHasContent("potential-duplicate", supportTask.TrnRequestMetadata!.PotentialDuplicate ? "Yes" : "No");

        void AssertRowHasContent(string testId, string expectedText)
        {
            var column = resultRow.GetElementByTestId(testId);
            Assert.NotNull(column);
            Assert.Equal(expectedText, column.TrimmedText());
        }
    }

    [Fact]
    public async Task Get_SearchByFirstName_ShowsMatchingResult()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync(name: "NPQ");
        var firstName = TestData.GenerateFirstName();
        var (supportTask, _, _) = await TestData.CreateNpqTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            configure: t => t.WithFirstName(firstName));

        var search = firstName;

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/npq-trn-requests/?Search={Uri.EscapeDataString(search)}");

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
        var applicationUser = await TestData.CreateApplicationUserAsync(name: "NPQ");
        var middleName = TestData.GenerateMiddleName();
        var (supportTask, _, _) = await TestData.CreateNpqTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            configure: t => t.WithMiddleName(middleName));

        var search = middleName;

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/npq-trn-requests/?Search={Uri.EscapeDataString(search)}");

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
        var applicationUser = await TestData.CreateApplicationUserAsync(name: "NPQ");
        var lastName = TestData.GenerateLastName();
        var (supportTask, _, _) = await TestData.CreateNpqTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            configure: t => t.WithLastName(lastName));

        var search = lastName;

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/npq-trn-requests/?Search={Uri.EscapeDataString(search)}");

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
        var applicationUser = await TestData.CreateApplicationUserAsync(name: "NPQ");
        var firstName = TestData.GenerateFirstName();
        var lastName = TestData.GenerateFirstName();
        var (supportTask, _, _) = await TestData.CreateNpqTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            configure: t => t.WithFirstName(firstName).WithLastName(lastName));

        var search = $"{firstName} {lastName}";

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/npq-trn-requests/?Search={Uri.EscapeDataString(search)}");

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
        var applicationUser = await TestData.CreateApplicationUserAsync(name: "NPQ");
        var emailAddress = TestData.GenerateUniqueEmail();
        var (supportTask, _, _) = await TestData.CreateNpqTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            configure: t => t.WithEmailAddress(emailAddress));

        var search = emailAddress;

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/npq-trn-requests/?Search={Uri.EscapeDataString(search)}");

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
        var applicationUser = await TestData.CreateApplicationUserAsync(name: "NPQ");
        var (supportTask, _, _) = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var search = supportTask.CreatedOn.ToString(dateFormat);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/npq-trn-requests/?Search={Uri.EscapeDataString(search)}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        doc.AssertResultsContainsTask(supportTask.SupportTaskReference);
    }

    [Fact]
    public async Task Get_NoSortParametersSpecified_ShowsRequestsOrderedByRequestedOnAscending()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync(name: "NPQ");

        var (supportTask1, _, _) = await TestData.CreateNpqTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t.WithCreatedOn(new DateTime(2025, 1, 1)));

        var (supportTask2, _, _) = await TestData.CreateNpqTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t.WithCreatedOn(new DateTime(2023, 10, 10)));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Collection(
            GetResultTaskReferences(doc),
            result => Assert.Equal(supportTask2.SupportTaskReference, result),
            result => Assert.Equal(supportTask1.SupportTaskReference, result));
    }

    [Fact]
    public async Task Get_SortByNameAscending_ShowsRequestsInCorrectOrder()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync(name: "NPQ");

        var (supportTask1, _, _) = await TestData.CreateNpqTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t.WithFirstName("Zavier"));

        var (supportTask2, _, _) = await TestData.CreateNpqTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t.WithFirstName("Aaron"));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/?sortBy={NpqTrnRequestsSortByOption.Name}&sortDirection={SortDirection.Ascending}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Collection(
            GetResultTaskReferences(doc),
            result => Assert.Equal(supportTask2.SupportTaskReference, result),
            result => Assert.Equal(supportTask1.SupportTaskReference, result));
    }

    [Fact]
    public async Task Get_SortByNameDescending_ShowsRequestsInCorrectOrder()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync(name: "NPQ");

        var (supportTask1, _, _) = await TestData.CreateNpqTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t.WithFirstName("Zavier"));

        var (supportTask2, _, _) = await TestData.CreateNpqTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t.WithFirstName("Aaron"));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/?sortBy={NpqTrnRequestsSortByOption.Name}&sortDirection={SortDirection.Descending}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Collection(
            GetResultTaskReferences(doc),
            result => Assert.Equal(supportTask1.SupportTaskReference, result),
            result => Assert.Equal(supportTask2.SupportTaskReference, result));
    }

    [Fact]
    public async Task Get_SortByEmailAscending_ShowsRequestsInCorrectOrder()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync(name: "NPQ");

        var (supportTask1, _, _) = await TestData.CreateNpqTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t.WithEmailAddress("zavier@example.com"));

        var (supportTask2, _, _) = await TestData.CreateNpqTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t.WithEmailAddress("aaron@example.com"));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/?sortBy={NpqTrnRequestsSortByOption.Email}&sortDirection={SortDirection.Ascending}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Collection(
            GetResultTaskReferences(doc),
            result => Assert.Equal(supportTask2.SupportTaskReference, result),
            result => Assert.Equal(supportTask1.SupportTaskReference, result));
    }

    [Fact]
    public async Task Get_SortByEmailDescending_ShowsRequestsInCorrectOrder()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync(name: "NPQ");

        var (supportTask1, _, _) = await TestData.CreateNpqTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t.WithEmailAddress("zavier@example.com"));

        var (supportTask2, _, _) = await TestData.CreateNpqTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t.WithEmailAddress("aaron@example.com"));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/?sortBy={NpqTrnRequestsSortByOption.Email}&sortDirection={SortDirection.Descending}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Collection(
            GetResultTaskReferences(doc),
            result => Assert.Equal(supportTask1.SupportTaskReference, result),
            result => Assert.Equal(supportTask2.SupportTaskReference, result));
    }

    [Fact]
    public async Task Get_SortByRequestedOnAscending_ShowsRequestsInCorrectOrder()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync(name: "NPQ");

        var (supportTask1, _, _) = await TestData.CreateNpqTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t.WithCreatedOn(new DateTime(2025, 1, 1)));

        var (supportTask2, _, _) = await TestData.CreateNpqTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t.WithCreatedOn(new DateTime(2023, 10, 10)));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/?sortBy={NpqTrnRequestsSortByOption.RequestedOn}&sortDirection={SortDirection.Ascending}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Collection(
            GetResultTaskReferences(doc),
            result => Assert.Equal(supportTask2.SupportTaskReference, result),
            result => Assert.Equal(supportTask1.SupportTaskReference, result));
    }

    [Fact]
    public async Task Get_SortByRequestedOnDescending_ShowsRequestsInCorrectOrder()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync(name: "NPQ");

        var (supportTask1, _, _) = await TestData.CreateNpqTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t.WithCreatedOn(new DateTime(2025, 1, 1)));

        var (supportTask2, _, _) = await TestData.CreateNpqTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t.WithCreatedOn(new DateTime(2023, 10, 10)));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/?sortBy={NpqTrnRequestsSortByOption.RequestedOn}&sortDirection={SortDirection.Descending}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Collection(
            GetResultTaskReferences(doc),
            result => Assert.Equal(supportTask1.SupportTaskReference, result),
            result => Assert.Equal(supportTask2.SupportTaskReference, result));
    }

    [Fact]
    public async Task Get_SortByPotentialDuplicateAscending_ShowsRequestsInCorrectOrder()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync(name: "NPQ");

        var (supportTask1, _, _) = await TestData.CreateNpqTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t =>
            {
                t.WithMatches(true);
            });

        var (supportTask2, _, _) = await TestData.CreateNpqTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t =>
            {
                t.WithMatches(false);
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/?sortBy={NpqTrnRequestsSortByOption.PotentialDuplicate}&sortDirection={SortDirection.Ascending}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Collection(
            GetResultTaskReferences(doc),
            result => Assert.Equal(supportTask2.SupportTaskReference, result),
            result => Assert.Equal(supportTask1.SupportTaskReference, result));
    }

    [Fact]
    public async Task Get_SortByPotentialDuplicateDescending_ShowsRequestsInCorrectOrder()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync(name: "NPQ");

        var (supportTask1, _, _) = await TestData.CreateNpqTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t =>
            {
                t.WithMatches(true);
            });

        var (supportTask2, _, _) = await TestData.CreateNpqTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t =>
            {
                t.WithMatches(false);
            });
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/?sortBy={NpqTrnRequestsSortByOption.PotentialDuplicate}&sortDirection={SortDirection.Descending}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Collection(
            GetResultTaskReferences(doc),
            result => Assert.Equal(supportTask1.SupportTaskReference, result),
            result => Assert.Equal(supportTask2.SupportTaskReference, result));
    }

    [Fact]
    public async Task Get_ShowsPageOfResults()
    {
        // Arrange
        var pageSize = 20;
        var page = 2;

        var applicationUser = await TestData.CreateApplicationUserAsync(name: "NPQ");

        // Create enough tasks to create 3 pages
        await AsyncEnumerable.ToArrayAsync(Enumerable.Range(1, (pageSize * page) + 1)
            .ToAsyncEnumerable()
            .SelectAwait(async _ => await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId)));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/?pageNumber={page}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(pageSize, GetResultTaskReferences(doc).Length);
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
