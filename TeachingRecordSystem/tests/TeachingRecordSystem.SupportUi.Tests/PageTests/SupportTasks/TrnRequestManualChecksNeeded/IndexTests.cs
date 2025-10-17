using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.TrnRequestManualChecksNeeded;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.TrnRequestManualChecksNeeded;

public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Test]
    public async Task Get_NoOpenTasks_ShowsNoTasksMessage()
    {
        // Arrange
        await CreateSupportTaskAsync(status: SupportTaskStatus.Closed);

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/manual-checks-needed");

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
        var supportTask = await CreateSupportTaskAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/manual-checks-needed");

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
        AssertRowHasContent("created-on", supportTask.CreatedOn.ToString(UiDefaults.DateOnlyDisplayFormat));
        AssertRowHasContent("date-of-birth", supportTask.TrnRequestMetadata!.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat));
        AssertRowHasContent("source", supportTask.TrnRequestMetadata.ApplicationUser!.Name);

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
        var firstName = TestData.GenerateFirstName();
        var supportTask = await CreateSupportTaskAsync(configureApiTrnRequest: t => t.WithFirstName(firstName));

        var search = firstName;

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/manual-checks-needed?Search={Uri.EscapeDataString(search)}");

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
        var middleName = TestData.GenerateMiddleName();
        var supportTask = await CreateSupportTaskAsync(configureApiTrnRequest: t => t.WithMiddleName(middleName));

        var search = middleName;

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/manual-checks-needed?Search={Uri.EscapeDataString(search)}");

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
        var lastName = TestData.GenerateLastName();
        var supportTask = await CreateSupportTaskAsync(configureApiTrnRequest: t => t.WithLastName(lastName));

        var search = lastName;

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/manual-checks-needed?Search={Uri.EscapeDataString(search)}");

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
        var firstName = TestData.GenerateFirstName();
        var lastName = TestData.GenerateFirstName();
        var supportTask = await CreateSupportTaskAsync(configureApiTrnRequest: t => t.WithFirstName(firstName).WithLastName(lastName));

        var search = $"{firstName} {lastName}";

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/manual-checks-needed?Search={Uri.EscapeDataString(search)}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        doc.AssertResultsContainsTask(supportTask.SupportTaskReference);
    }

    [Test]
    public async Task Get_NoSortParametersSpecified_ShowsRequestsOrderedByCreatedOnAscending()
    {
        // Arrange
        var supportTask1 = await CreateSupportTaskAsync(createdOn: new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        var supportTask2 = await CreateSupportTaskAsync(createdOn: new DateTime(2023, 10, 10, 0, 0, 0, DateTimeKind.Utc));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/manual-checks-needed");

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
        var supportTask1 = await CreateSupportTaskAsync(configureApiTrnRequest: t => t.WithFirstName("Zavier"));

        var supportTask2 = await CreateSupportTaskAsync(configureApiTrnRequest: t => t.WithFirstName("Aaron"));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/manual-checks-needed?sortBy={TrnRequestManualChecksNeededSortByOption.Name}&sortDirection={SortDirection.Ascending}");

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
        var supportTask1 = await CreateSupportTaskAsync(configureApiTrnRequest: t => t.WithFirstName("Zavier"));

        var supportTask2 = await CreateSupportTaskAsync(configureApiTrnRequest: t => t.WithFirstName("Aaron"));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/manual-checks-needed?sortBy={TrnRequestManualChecksNeededSortByOption.Name}&sortDirection={SortDirection.Descending}");

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
    public async Task Get_SortByDateOfBirthAscending_ShowsRequestsInCorrectOrder()
    {
        // Arrange
        var supportTask1 = await CreateSupportTaskAsync(configureApiTrnRequest: t => t.WithDateOfBirth(new(2025, 1, 1)));

        var supportTask2 = await CreateSupportTaskAsync(configureApiTrnRequest: t => t.WithDateOfBirth(new(2023, 10, 10)));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/manual-checks-needed?sortBy={TrnRequestManualChecksNeededSortByOption.DateOfBirth}&sortDirection={SortDirection.Ascending}");

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
    public async Task Get_SortByDateOfBirthDescending_ShowsRequestsInCorrectOrder()
    {
        // Arrange
        var supportTask1 = await CreateSupportTaskAsync(configureApiTrnRequest: t => t.WithDateOfBirth(new(2025, 1, 1)));

        var supportTask2 = await CreateSupportTaskAsync(configureApiTrnRequest: t => t.WithDateOfBirth(new(2023, 10, 10)));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/manual-checks-needed?sortBy={TrnRequestManualChecksNeededSortByOption.DateOfBirth}&sortDirection={SortDirection.Descending}");

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
    public async Task Get_SortByCreatedOnAscending_ShowsRequestsInCorrectOrder()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask1 = await CreateSupportTaskAsync(createdOn: new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        var supportTask2 = await CreateSupportTaskAsync(createdOn: new DateTime(2023, 10, 10, 0, 0, 0, DateTimeKind.Utc));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/manual-checks-needed?sortBy={TrnRequestManualChecksNeededSortByOption.DateCreated}&sortDirection={SortDirection.Ascending}");

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
    public async Task Get_SortByCreatedOnDescending_ShowsRequestsInCorrectOrder()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask1 = await CreateSupportTaskAsync(createdOn: new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        var supportTask2 = await CreateSupportTaskAsync(createdOn: new DateTime(2023, 10, 10, 0, 0, 0, DateTimeKind.Utc));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/manual-checks-needed?sortBy={TrnRequestManualChecksNeededSortByOption.DateCreated}&sortDirection={SortDirection.Descending}");

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

        var supportTask1 = await CreateSupportTaskAsync(applicationUser1.UserId);

        var supportTask2 = await CreateSupportTaskAsync(applicationUser2.UserId);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/manual-checks-needed?sortBy={TrnRequestManualChecksNeededSortByOption.Source}&sortDirection={SortDirection.Ascending}");

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

        var supportTask1 = await CreateSupportTaskAsync(applicationUser1.UserId);

        var supportTask2 = await CreateSupportTaskAsync(applicationUser2.UserId);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/manual-checks-needed?sortBy={TrnRequestManualChecksNeededSortByOption.Source}&sortDirection={SortDirection.Descending}");

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

        // Create enough tasks to create 3 pages
        var tasks = await Enumerable.Range(1, (pageSize * page) + 1)
            .ToAsyncEnumerable()
            .SelectAwait(async _ => await CreateSupportTaskAsync())
            .ToArrayAsync();

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/manual-checks-needed?pageNumber={page}");

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

        await CreateSupportTaskAsync(applicationUserWithShortName.UserId);
        Clock.Advance();
        await CreateSupportTaskAsync(applicationUserWithoutShortName.UserId);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/manual-checks-needed?sortBy={TrnRequestManualChecksNeededSortByOption.DateCreated}");

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

    private async Task<SupportTask> CreateSupportTaskAsync(
        Guid? applicationUserId = null,
        SupportTaskStatus status = SupportTaskStatus.Open,
        DateTime? createdOn = null,
        Action<TestData.CreateApiTrnRequestSupportTaskBuilder>? configureApiTrnRequest = null)
    {
        var matchedPerson = await TestData.CreatePersonAsync(p => p.WithEmailAddress(TestData.GenerateUniqueEmail()).WithAlert().WithQts().WithEyts());

        if (applicationUserId is null)
        {
            var applicationUser = await TestData.CreateApplicationUserAsync();
            applicationUserId = applicationUser.UserId;
        }

        var apiSupportTask = await TestData.CreateResolvedApiTrnRequestSupportTaskAsync(
            applicationUserId.Value,
            matchedPerson.Person,
            t =>
            {
                t.WithTrnRequestStatus(TrnRequestStatus.Pending);
                configureApiTrnRequest?.Invoke(t);
            });

        return await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(
            applicationUserId.Value,
            apiSupportTask.TrnRequestMetadata!.RequestId,
            status,
            createdOn);
    }
}

file static class Extensions
{
    public static void AssertResultsContainsTask(this IHtmlDocument document, string supportTaskReference) =>
        Assert.NotNull(document.GetElementByTestId($"task:{supportTaskReference}"));
}
