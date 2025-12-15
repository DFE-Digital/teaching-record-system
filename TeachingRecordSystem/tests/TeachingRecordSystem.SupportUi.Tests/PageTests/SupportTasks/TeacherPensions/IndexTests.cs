using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Services.SupportTasks;
using TeachingRecordSystem.SupportUi.Tests.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.TeacherPensions;

[ClearDbBeforeTest, Collection(nameof(DisableParallelization))]
public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_NoTasks_ShowsNoTasksMessage()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/teacher-pensions?_f=1");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.NotNull(doc.GetElementByTestId("no-tasks-message"));
        Assert.Null(doc.GetElementByTestId("no-results-message"));
        Assert.Null(doc.GetElementByTestId("results"));
    }

    [Fact]
    public async Task Get_WithSupportTask_RendersResults()
    {
        // Arrange
        await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
            fileName: "SomeFileName.txt",
            integrationTransactionId: 1,
            createdOn: Clock.UtcNow,
            configurePerson: p => p.WithFirstName("John").WithMiddleName("Maynard").WithLastName("Smith").WithNationalInsuranceNumber());

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/teacher-pensions?_f=1");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Null(doc.GetElementByTestId("no-tasks-message"));
        Assert.Null(doc.GetElementByTestId("no-results-message"));

        var resultRow = GetResultRows(doc).FirstOrDefault();
        Assert.NotNull(resultRow);

        AssertRowHasContent("name", "John Maynard Smith");
        AssertRowHasContent("filename", "SomeFileName.txt");
        AssertRowHasContent("integration-transaction-id", "1");
        AssertRowHasContent("created-on", Clock.UtcNow.ToString("dd MMM yyyy"));

        void AssertRowHasContent(string testId, string expectedText)
        {
            var column = resultRow.GetElementByTestId(testId);
            Assert.NotNull(column);
            Assert.Equal(expectedText, column.TrimmedText());
        }
    }

    [Theory]
    [InlineData(TeachersPensionsPotentialDuplicatesSortByOption.Name, SortDirection.Ascending, new[] { "ST2", "ST1" })]
    [InlineData(TeachersPensionsPotentialDuplicatesSortByOption.Name, SortDirection.Descending, new[] { "ST1", "ST2" })]
    [InlineData(TeachersPensionsPotentialDuplicatesSortByOption.CreatedOn, SortDirection.Ascending, new[] { "ST2", "ST1" })]
    [InlineData(TeachersPensionsPotentialDuplicatesSortByOption.CreatedOn, SortDirection.Descending, new[] { "ST1", "ST2" })]
    [InlineData(TeachersPensionsPotentialDuplicatesSortByOption.Filename, SortDirection.Ascending, new[] { "ST2", "ST1" })]
    [InlineData(TeachersPensionsPotentialDuplicatesSortByOption.Filename, SortDirection.Descending, new[] { "ST1", "ST2" })]
    [InlineData(TeachersPensionsPotentialDuplicatesSortByOption.InterfaceId, SortDirection.Ascending, new[] { "ST2", "ST1" })]
    [InlineData(TeachersPensionsPotentialDuplicatesSortByOption.InterfaceId, SortDirection.Descending, new[] { "ST1", "ST2" })]
    public async Task Get_SortBy_ShowsRequestsInCorrectOrder(TeachersPensionsPotentialDuplicatesSortByOption sortBy, SortDirection sortDirection, string[] taskKeys)
    {
        // Arrange
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
                createdOn: new DateTime(2025, 1, 1),
                fileName: "zzzzzz.csv",
                integrationTransactionId: 100,
                configurePerson: p => p
                    .WithFirstName("Zavier")
                    .WithDateOfBirth(new(2025, 1, 1))),

            ["ST2"] = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
                createdOn: new DateTime(2023, 10, 10),
                fileName: "aaaaa.txt",
                integrationTransactionId: 2,
                configurePerson: p => p
                    .WithFirstName("Aaron")
                    .WithDateOfBirth(new(2023, 10, 10)))
        };

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"/support-tasks/teacher-pensions/?sortBy={sortBy}&sortDirection={sortDirection}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(taskKeys, GetResultTaskKeys(doc, tasks));
    }

    [Fact]
    public async Task Get_ShowsPageOfResults()
    {
        // Arrange
        var pageSize = 20;
        var page = 2;

        // Create enough tasks to create 3 pages
        var tasks = await AsyncEnumerable.ToArrayAsync(Enumerable.Range(1, (pageSize * page) + 1)
                .ToAsyncEnumerable()
                .SelectAwait(async _ => await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync()));

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"/support-tasks/teacher-pensions/?pageNumber={page}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(pageSize, GetResultTaskReferences(doc).Length);
    }

    private static IElement[] GetResultRows(IHtmlDocument document) =>
        document
            .GetElementByTestId("results")!
            .GetElementsByClassName("govuk-table__row")
            .ToArray();

    private static string[] GetResultTaskReferences(IHtmlDocument document) =>
        GetResultRows(document)
            .Select(row => row.GetAttribute("data-testid")!["task:".Length..])
            .ToArray();

    private static string[] GetResultTaskKeys(IHtmlDocument document, SupportTaskLookup tasks) =>
        GetResultTaskReferences(document)
            .Select(tasks.GetKeyFor)
            .ToArray();
}
